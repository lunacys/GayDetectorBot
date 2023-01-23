using System.Reflection;
using GayDetectorBot.WebApi.Models.Tg;
using GayDetectorBot.WebApi.Services.Tg.Helpers;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public class MessageHandlerService : IMessageHandlerService
{
    private readonly ILogger<MessageHandlerService> _logger;
    private readonly ICommandMapService _commandMap;
    
    private readonly IEnumerable<(Type Type, MessageHandlerAttribute Metadata)> _handlerTypes;

    private readonly IServiceProvider _serviceProvider;
    private readonly IHandlerMetadataContainer _handlerMetadataContainer;
    private readonly IJsEvaluatorService _jsEvaluator;

    private readonly string _helpMessage;

    public MessageHandlerService(
        ILogger<MessageHandlerService> logger, 
        ICommandMapService commandMap, 
        IHandlerMetadataContainer handlerMetadataContainer, 
        IServiceProvider serviceProvider,
        IJsEvaluatorService jsEvaluator)
    {
        _logger = logger;
        _commandMap = commandMap;
        _handlerMetadataContainer = handlerMetadataContainer;
        _serviceProvider = serviceProvider;
        _jsEvaluator = jsEvaluator;

        _handlerTypes = _handlerMetadataContainer.GetHandlerTypes();

        _helpMessage = GetHelpMessage();
    }
    
    public async Task Message(Message? message, ITelegramBotClient client)
    {
        if (message == null)
            return;

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
            await client.SendTextMessageAsync(chatId, _helpMessage, ParseMode.Markdown);
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

                        await Handle(handlerType.Type, chatId, client, message, paramList.ToArray());
                    }
                    else
                    {
                        await Handle(handlerType.Type, chatId, client, message, data?.Trim() ?? null);
                    }
                }
                catch (TelegramCommandException e)
                {
                    await client.SendTextMessageAsync(chatId, "Ошибка: " + e.Message, ParseMode.Markdown);
                }
                catch (Exception e)
                {
                    _logger.LogError(e.ToString());
                    await client.SendTextMessageAsync(chatId, "Непредвиденная ошибка: " + e.Message,
                        ParseMode.Markdown);
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

                if (c.ToLower() != "@gamee")
                    c = c.Replace("@", "");

                if (c.StartsWith("!eval "))
                {
                    var evalContent = c.Replace("!eval ", "");
                    var res = await _jsEvaluator.EvaluateAsync(evalContent, engine =>
                    {
                        engine.SetValue("SendCommand", async (string snd) =>
                            {
                                var cmd = _commandMap.GetByChatId(chatId).Find(co => co.Prefix == snd);
                                if (cmd != null)
                                    await client.SendTextMessageAsync(chatId, cmd.Content, ParseMode.Html);
                            }
                        );
                    });

                    await client.SendTextMessageAsync(message.Chat.Id, res ?? "пусто", ParseMode.Html,
                        replyToMessageId: message.MessageId);
                }
                else
                {
                    await client.SendTextMessageAsync(message.Chat.Id, c, ParseMode.Html,
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