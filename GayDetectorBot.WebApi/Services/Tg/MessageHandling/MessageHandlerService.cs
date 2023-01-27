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
    
    private readonly IEnumerable<(Type Type, MessageHandlerAttribute Metadata)> _handlerTypes;

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

        _logger.LogInformation(
            $"Handling message '{message.Text}' " +
            $"of type '{message.Type}' " +
            $"in chat '{message.Chat.Id}' " +
            $"from '{message.From?.Username}' ({message.From?.Id})"
        );

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
        var fileInfo = await client.GetFileAsync(fileId);
        var filePath = fileInfo.FilePath;

        if (filePath == null)
            return;

        var extension = Path.GetExtension(filePath);

        string dest = basePath + Utils.GenerateRandomString() + extension;

        Directory.CreateDirectory(Path.GetDirectoryName(dest));

        await using Stream fileStream = File.OpenWrite(dest);
        await client.DownloadFileAsync(filePath, fileStream);
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
            await client.SendTextMessageAsync(chatId, _helpMessage, parseMode: ParseMode.Markdown);
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
            if (handlerType.Metadata.CommandAlias == command)
            {
                try
                {
                    if (!(await CheckPermissions(handlerType.Metadata, client, chatId, message)))
                        throw new TelegramCommandException("А тебе низя такое делать");

                    if (handlerType.Metadata.HasParameters && handlerType.Metadata.ParameterCount > 1)
                    {
                        var paramList = new List<string>(handlerType.Metadata.ParameterCount);
                        var lastIndex = 0;
                        var curIndex = 0;

                        // Parsing parameters
                        while (data != null && lastIndex < data.Length)
                        {
                            if (curIndex + 1 >= handlerType.Metadata.ParameterCount)
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

                        _logger.LogInformation($"Executing command '{handlerType.Metadata.CommandAlias}'");
                        await Handle(handlerType.Type, chatId, client, message, paramList.ToArray());
                    }
                    else // ParameterCount <= 1
                    {
                        _logger.LogInformation($"Executing command '{handlerType.Metadata.CommandAlias}'");
                        await Handle(handlerType.Type, chatId, client, message, data?.Trim() ?? "");
                    }
                }
                catch (TelegramCommandException e)
                {
                    await client.SendTextMessageAsync(chatId, "Ошибка: " + e.Message, parseMode: ParseMode.Markdown);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    await client.SendTextMessageAsync(chatId, "Непредвиденная ошибка: " + e.Message,
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

                    var evalContent = c.Replace("!eval ", "");
                    var res = await _jsEvaluator.EvaluateAsync(evalContent, engine =>
                    {
                        engine.SetValue("SendCommand", async (string snd) =>
                            {
                                var cmd = _commandMap.GetByChatId(chatId).Find(co => co.Prefix == snd);
                                if (cmd != null)
                                    await client.SendTextMessageAsync(chatId, cmd.Content, parseMode: ParseMode.Html);
                            }
                        );
                    });

                    await client.SendTextMessageAsync(message.Chat.Id, res ?? "пусто", parseMode: ParseMode.Html,
                        replyToMessageId: message.MessageId);
                }
                else
                {
                    _logger.LogInformation($"Executing custom command '{lower}' with content: {c}");

                    await client.SendTextMessageAsync(message.Chat.Id, c, parseMode: ParseMode.Html,
                        replyToMessageId: message.MessageId);
                }
            }
        }
    }

    private async Task Handle(Type type, long chatId, ITelegramBotClient client, Message message, params string[] data)
    {
        var service = _serviceProvider.GetMessageHandler(type, chatId, client);
        await service.HandleAsync(message, data);
    }

    private async Task<bool> CheckPermissions(MessageHandlerAttribute metadata, ITelegramBotClient client, long chatId, Message message)
    {
        var permissions = metadata.Permission;
        var user = await client.GetChatMemberAsync(chatId, message.From!.Id);

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
        var result = "";

        for (int i = startIndex; i < str.Length; i++)
        {
            if (str[i] == ' ' || str[i] == '\n')
                return (result, i);

            result += str[i];
        }

        return (result, str.Length);
    }

    private string GetHelpMessage()
    {
        var mhc = _handlerTypes;

        var result = "Список основных команд:\n\n";

        foreach (var data in mhc)
        {
            result += "`!";

            if (data.Metadata.HasParameters)
            {
                result += $"{data.Metadata.CommandAlias} ";
                result += string.Join(' ', data.Metadata.Parameters.Select(s => $"<{s}>"));
                result += "` - " + data.Metadata.Description;
            }
            else
            {
                result += $"{data.Metadata.CommandAlias}` - {data.Metadata.Description}";
            }

            result += "\n";
        }

        return result;
    }
}