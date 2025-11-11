using System.Text;
using GayDetectorBot.WebApi.Models.Tg;
using GayDetectorBot.WebApi.Services.Tg.Helpers;
using GayDetectorBot.WebApi.Tg;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using File = System.IO.File;
using Update = Telegram.Bot.Types.Update;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public interface IMessageHandlerService
{
    Task Update(Update? update, ITelegramBotClient client);
}

public class MessageHandlerService : IMessageHandlerService
{
    private readonly ILogger<MessageHandlerService> _logger;
    private readonly ICommandMapService _commandMap;
    
    private readonly IEnumerable<(Type Type, HandlerMetadata Metadata)> _handlerTypes;

    private readonly IServiceProvider _serviceProvider;
    private readonly IJsEvaluatorService _jsEvaluator;
    private readonly ISavedFileContainer _savedFileContainer;

    private readonly string _helpMessage;

    public MessageHandlerService(
        ILogger<MessageHandlerService> logger, 
        ICommandMapService commandMap, 
        IHandlerMetadataContainer handlerMetadataContainer, 
        IServiceProvider serviceProvider,
        IJsEvaluatorService jsEvaluator,
        ISavedFileContainer savedFileContainer)
    {
        _logger = logger;
        _commandMap = commandMap;
        _serviceProvider = serviceProvider;
        _jsEvaluator = jsEvaluator;
        _savedFileContainer = savedFileContainer;

        _handlerTypes = handlerMetadataContainer.GetHandlerTypes();

        _helpMessage = GetHelpMessage();

        Directory.CreateDirectory("downloads");
    }
    
    public async Task Update(Update? update, ITelegramBotClient client)
    {
        if (update == null)
            return;

        if (update.Type == UpdateType.Message)
            await HandleMessage(update.Message, client);
    }

    private async Task HandleMessage(Message? message, ITelegramBotClient client)
    {
        if (message == null)
            return;

        if (message.From?.IsBot ?? true)
            return;

        /*_logger.LogInformation(
            $"Handling message '{message.Text}' " +
            $"of type '{message.Type}' " +
            $"in chat '{message.Chat.Id}' " +
            $"from '{message.From?.Username}' ({message.From?.Id})"
        );*/

        var task = message.Type switch
        {
            MessageType.Photo => HandlePhoto(message),
            MessageType.Audio => HandleAudio(message),
            MessageType.Video => HandleVideo(message),
            MessageType.Voice => HandleVoice(message),
            MessageType.Document => HandleDocument(message),
            MessageType.Sticker => HandleSticker(message),
            _ => Task.CompletedTask
        };
        await task;

        // We want to handle text in any case.
        await HandleText(message, client);
    }

    private async Task HandlePhoto(Message message)
    {
        if (message.Photo == null || message.Photo.Length == 0)
            return;

        var pid = message.Photo.Last().FileId;

        await _savedFileContainer.Save(new SavedFile
        {
            FileId = pid,
            ChatId = message.Chat.Id,
            Type = SavedFileType.Photo
        });
    }

    private async Task HandleVideo(Message message)
    {
        if (message.Video == null)
            return;

        var pid = message.Video.FileId;

        await _savedFileContainer.Save(new SavedFile
        {
            FileId = pid,
            ChatId = message.Chat.Id,
            Type = SavedFileType.Video
        });
    }

    private async Task HandleDocument(Message message)
    {
        if (message.Document == null)
            return;

        var pid = message.Document.FileId;

        await _savedFileContainer.Save(new SavedFile
        {
            FileId = pid,
            ChatId = message.Chat.Id,
            Type = SavedFileType.Document
        });
    }

    private async Task HandleAudio(Message message)
    {
        if (message.Audio == null)
            return;

        var pid = message.Audio.FileId;

        await _savedFileContainer.Save(new SavedFile
        {
            FileId = pid,
            ChatId = message.Chat.Id,
            Type = SavedFileType.Audio
        });
    }

    private async Task HandleSticker(Message message)
    {
        if (message.Sticker == null)
            return;

        await Task.CompletedTask;
    }

    private async Task HandleVoice(Message message)
    {
        if (message.Voice == null)
            return;

        var pid = message.Voice.FileId;

        await _savedFileContainer.Save(new SavedFile
        {
            FileId = pid,
            ChatId = message.Chat.Id,
            Type = SavedFileType.Voice
        });
    }

    private async Task DownloadFile(string fileId, string basePath, ITelegramBotClient client)
    {
        var fileInfo = await client.GetFile(fileId);
        var filePath = fileInfo.FilePath;

        if (filePath == null)
            return;

        var extension = Path.GetExtension(filePath);

        string dest = basePath + Utils.GenerateRandomString() + extension;

        Directory.CreateDirectory(Path.GetDirectoryName(dest) ?? "");

        await using Stream fileStream = File.OpenWrite(dest);
        await client.DownloadFile(filePath, fileStream);
    }

    private async Task HandleText(Message message, ITelegramBotClient client)
    {
        var text = message.Text;

        if (string.IsNullOrEmpty(text))
            return;

        if (message.From == null)
            return;

        if (!text.StartsWith('!'))
            return;

        var chatId = message.Chat.Id;

        var lower = text.ToLower().Trim();

        if (lower == "!помоги")
        {
            await client.SendMessage(chatId, _helpMessage, parseMode: ParseMode.Markdown);
            return;
        }

        // Take the first word which will be the command, it must start with ! and end with a whitespace
        var whitespaceIndex = lower.IndexOfAny(new[] { ' ', '\n' });
        var command = lower;
        if (whitespaceIndex >= 0)
            command = lower.Substring(0, whitespaceIndex);
        // Remove the ! symbol from the beginning
        command = command.Remove(0, 1);

        string? data;

        var i = message.Text!.IndexOfAny(new[] { ' ', '\n' });
        if (i > 0)
            data = message.Text.Substring(i + 1);
        else
            data = null;

        await ProcessCommand(command, client, chatId, message, data);
        await CheckCustomCommand(chatId, lower, client, message);
    }

    private async Task ProcessCommand(string command, ITelegramBotClient client, long chatId, Message message, string? data)
    {
        foreach (var handlerType in _handlerTypes)
        {
            if (handlerType.Metadata.Common.CommandAlias == command)
            {
                try
                {
                    if (!(await CheckPermissions(handlerType.Metadata, client, chatId, message)))
                        throw new TelegramCommandException("А тебе низя такое делать");

                    if (handlerType.Metadata.Common.HasParameters && handlerType.Metadata.Common.ParameterCount > 1)
                    {
                        var paramList = new List<string>(handlerType.Metadata.Common.ParameterCount);
                        var lastIndex = 0;
                        var curIndex = 0;

                        // Parsing parameters
                        while (data != null && lastIndex < data.Length)
                        {
                            if (curIndex + 1 >= handlerType.Metadata.Common.ParameterCount)
                            {
                                var str = data.Substring(lastIndex);
                                paramList.Add(str);
                                lastIndex = data.Length;
                            }
                            else
                            {
                                var res = ReadUntilWhitespace(data, lastIndex);
                                paramList.Add(res.Item1);
                                lastIndex = res.Item2 + 1;
                                curIndex++;
                            }
                        }

                        _logger.LogInformation($"Executing command '{handlerType.Metadata.Common.CommandAlias}'");
                        await Handle(handlerType.Type, chatId, client, message, paramList.ToArray());
                    }
                    else // ParameterCount <= 1
                    {
                        _logger.LogInformation($"Executing command '{handlerType.Metadata.Common.CommandAlias}'");
                        await Handle(handlerType.Type, chatId, client, message, data?.Trim() ?? "");
                    }
                }
                catch (TelegramCommandException e)
                {
                    await client.SendMessage(chatId, "Ошибка: " + e.Message, parseMode: ParseMode.Markdown);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    await client.SendMessage(chatId, "Непредвиденная ошибка: " + e.Message,
                        parseMode: ParseMode.Markdown);
                }

                return;
            }
        }
    }

    private async Task CheckCustomCommand(long chatId, string lower, ITelegramBotClient client, Message message)
    {
        if (_commandMap.ContainsKey(chatId))
        {
            var content = _commandMap.GetByChatId(chatId)
                .FirstOrDefault(pc => pc.Prefix.ToLower() == lower);

            if (content != null)
            {
                var c = content.Content;
                if (string.IsNullOrEmpty(c))
                    return;

                // Removing user tags except for @gamee
                if (c.ToLower() != "@gamee")
                    c = c.Replace("@", "");

                // If a command starts with '!eval' it is a JS script, so we'll need to evaluate it
                if (c.StartsWith("!eval "))
                {
                    _logger.LogInformation($"Evaluating custom JS command '{lower}' with content: {c}");

                    /*var evalContent = c.Replace("!eval ", "");
                    var res = await _jsEvaluator.EvaluateAsync(evalContent, engine =>
                    {
                        engine.SetValue("SendCommand", async (string snd) =>
                            {
                                var cmd = _commandMap.GetByChatId(chatId).Find(co => co.Prefix == snd);
                                if (cmd != null)
                                    //await client.SendTextMessageAsync(chatId, cmd.Content, parseMode: ParseMode.Html);
                                    await client.SendMessage(chatId, cmd.Content, parseMode: ParseMode.Html);
                            }
                        );
                    });*/

                    //await client.SendMessage(message.Chat.Id, res ?? "Пусто", parseMode: ParseMode.Html, replyParameters: new ReplyParameters { MessageId = message.MessageId });
                    await client.SendMessage(message.Chat.Id, "Пока не работает", parseMode: ParseMode.Html, replyParameters: new ReplyParameters { MessageId = message.MessageId });
                }
                else
                {
                    _logger.LogInformation($"Executing custom command '{lower}' with content: {c}");

                    await client.SendMessage(message.Chat.Id, c, parseMode: ParseMode.Html,
                        replyParameters: new ReplyParameters() { MessageId = message.MessageId });
                }
            }
        }
    }

    private async Task Handle(Type type, long chatId, ITelegramBotClient client, Message message, params string[] data)
    {
        var service = _serviceProvider.GetMessageHandler(type, chatId, client);
        await service.HandleAsync(message, data);
    }

    private async Task<bool> CheckPermissions(HandlerMetadata metadata, ITelegramBotClient client, long chatId, Message message)
    {
        var permissions = metadata.Permission?.Permission ?? MemberStatusPermission.Creator | MemberStatusPermission.Administrator;
        var user = await client.GetChatMember(chatId, message.From!.Id);

        _logger.LogInformation($"Permissions of user {message.From.Username} ({message.From.Id}): {user.Status}");

        return HasPermissions(user.Status, permissions);
    }

    private static bool HasPermissions(ChatMemberStatus status, MemberStatusPermission permissions)
    {
        switch (status)
        {
            case ChatMemberStatus.Creator:
                return permissions.HasFlag(MemberStatusPermission.Creator);
            case ChatMemberStatus.Administrator:
                return permissions.HasFlag(MemberStatusPermission.Administrator);
            case ChatMemberStatus.Member:
                return permissions.HasFlag(MemberStatusPermission.Member);
            case ChatMemberStatus.Left:
                return permissions.HasFlag(MemberStatusPermission.Left);
            case ChatMemberStatus.Kicked:
                return permissions.HasFlag(MemberStatusPermission.Kicked);
            case ChatMemberStatus.Restricted:
                return permissions.HasFlag(MemberStatusPermission.Restricted);
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, null);
        }
    }

    private static (string, int) ReadUntilWhitespace(string str, int startIndex = 0)
    {
        var result = new StringBuilder();

        for (int i = startIndex; i < str.Length; i++)
        {
            if (str[i] == ' ' || str[i] == '\n')
                return (result.ToString(), i);

            result.Append(str[i]);
        }

        return (result.ToString(), str.Length);
    }

    private string GetHelpMessage()
    {
        var mhc = _handlerTypes.ToList();

        var result = new StringBuilder("Список основных команд:\n\n");

        var categories = new string[]
        {
            CommandCategories.Commands,
            CommandCategories.Gays,
            CommandCategories.Common,
            CommandCategories.DickSize
        };

        foreach (var category in categories)
        {
            var cmds =
                mhc
                    .Select(mh => mh.Metadata)
                    .Where(mh =>
                        mh.Metadata != null && mh.Metadata.Category == category ||
                        mh.Metadata == null && category == CommandCategories.Common)
                    .ToList();

            if (cmds.Count == 0)
                continue;
            
            result.Append($" > **{category}**: \n\n");

            foreach (var data in cmds)
            {
                result.Append(" - `!");

                if (data.Common.HasParameters)
                {
                    result.Append($"{data.Common.CommandAlias} ");
                    result.Append(string.Join(' ', data.Common.Parameters.Select(s => $"<{s}>")));
                    if (data.Metadata != null)
                        result.Append("` - " + data.Metadata.Description);
                }
                else
                {
                    result.Append($"{data.Common.CommandAlias}`");
                    if (data.Metadata != null)
                        result.Append($" - {data.Metadata.Description}");
                }

                result.Append("\n");
            }

            result.Append("==============\n\n");
        }

        return result.ToString();
    }
}