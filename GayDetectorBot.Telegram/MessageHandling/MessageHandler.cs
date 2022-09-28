using System.Reflection;
using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandling
{
    public class MessageHandler
    {
        private readonly CommandMap? _commandMap;
        private readonly RepositoryContainer _repositoryContainer;

        private readonly Dictionary<long, MessageHandlerContainer> _messageHandlerMap;

        private readonly List<(Type, MessageHandlerAttribute)> _handlerTypes;

        public MessageHandler(
            CommandRepository commandRepository, 
            GayRepository gayRepository,
            ChatRepository chatRepository, 
            ParticipantRepository participantRepository)
        {
            var cr = commandRepository;

            var assembly = Assembly.GetExecutingAssembly();
            _handlerTypes = GetTypesWithAttribute(assembly).ToList();

            List<string> reservedCommands = _handlerTypes.Select(tuple => "!" + tuple.Item2.CommandAlias.TrimEnd()).ToList();

            _commandMap = new CommandMap(cr);
            _commandMap.Initialize().Wait();

            _repositoryContainer = new RepositoryContainer(
                chatRepository,
                cr,
                gayRepository,
                participantRepository,
                _commandMap,
                reservedCommands
            );

            _messageHandlerMap = new Dictionary<long, MessageHandlerContainer>();
        }

        private void InitializeChat(long chatId, ITelegramBotClient client)
        {
            _messageHandlerMap[chatId] = new MessageHandlerContainer(_repositoryContainer, chatId, client);
            
            foreach (var valueTuple in _handlerTypes)
            {
                var mh = Activator.CreateInstance(valueTuple.Item1, _repositoryContainer) as IMessageHandler;
                if (mh == null)
                    throw new Exception("Could not create an instance");

                _messageHandlerMap[chatId].Register(mh);
            }
        }

        public async Task Message(Message? message, ITelegramBotClient client)
        {
            if (message == null)
                return;

            if (string.IsNullOrEmpty(message.Text))
                return;

            if (!message.Text.StartsWith("!"))
                return;

            if (message.From == null)
                return;

            var chatId = message.Chat.Id;

            if (!_messageHandlerMap.ContainsKey(chatId))
            {
                InitializeChat(chatId, client);
            }
               
            var lower = message.Text.ToLower().Trim();

            if (lower == "!помоги")
            {
                await client.SendTextMessageAsync(chatId, GetHelpMessageForChat(chatId), ParseMode.Markdown);
                return;
            }

            // Take the first word which will be the command, it must start with ! and end with a whitespace
            var whitespaceIndex = lower.IndexOfAny(new []{' ', '\n'});

            var command = lower;

            if (whitespaceIndex >= 0)
                command = lower.Substring(0, whitespaceIndex);

            // Remove the ! symbol from the beginning
            command = command.Remove(0, 1);

            string? data;

            var i = message.Text.IndexOfAny(new[] { ' ', '\n' });
            if (i > 0)
                data = message.Text.Substring(i + 1);
            else
                data = null;

            var mhc = _messageHandlerMap[chatId];

            foreach (var handlerData in mhc)
            {
                if (handlerData.Metadata.CommandAlias == command)
                {
                    try
                    {
                        var permissions = handlerData.Metadata.Permission;
                        var user = await client.GetChatMemberAsync(chatId, message.From.Id);

                        if (!HasPermissions(user.Status, permissions))
                            throw new TelegramCommandException("А тебе низя такое делать");

                        if (handlerData.Metadata.HasParameters && handlerData.Metadata.ParameterCount > 1)
                        {
                            var paramList = new List<string>(handlerData.Metadata.ParameterCount);

                            var lastIndex = 0;
                            var curIndex = 0;

                            while (data != null && lastIndex < data.Length)
                            {
                                if (curIndex + 1 >= handlerData.Metadata.ParameterCount)
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
                            
                            await handlerData.Handler.HandleAsync(message, paramList.ToArray());
                        }
                        else
                        {
                            await handlerData.Handler.HandleAsync(message, data?.Trim() ?? null);
                        }
                    }
                    catch (TelegramCommandException e)
                    {
                        await client.SendTextMessageAsync(chatId, "Ошибка: " + e.Message, ParseMode.Markdown, replyToMessageId: message.MessageId);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        await client.SendTextMessageAsync(chatId, "Непредвиденная ошибка: " + e.Message,
                            ParseMode.Markdown, replyToMessageId: message.MessageId);
                    }

                    return;
                }
            }
            
            // Checking if command is a custom command
            if (_commandMap!.ContainsKey(message.Chat.Id))
            {
                var content = _commandMap[message.Chat.Id]
                    .FirstOrDefault(prefixContent => prefixContent.Prefix.ToLower() == message.Text.ToLower());

                if (content != null)
                {
                    var c = content.Content;
                    if (string.IsNullOrEmpty(c))
                        return;

                    if (c.ToLower() != "@gamee")
                        c = c.Replace("@", "");
                    await client.SendTextMessageAsync(message.Chat.Id, c, ParseMode.Markdown, replyToMessageId: message.MessageId);
                }
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

        private static IEnumerable<(Type type, MessageHandlerAttribute attribute)> GetTypesWithAttribute(Assembly assembly)
        {
            foreach (var type in assembly.GetTypes())
            {
                var attr = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);

                if (attr.Length > 0)
                    yield return (type, attr[0] as MessageHandlerAttribute)!;
            }
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

        private string GetHelpMessageForChat(long chatId)
        {
            var mhc = _messageHandlerMap[chatId];

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
}