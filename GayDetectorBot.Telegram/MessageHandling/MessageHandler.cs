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
            var commandRepository1 = commandRepository;

            var assembly = Assembly.GetExecutingAssembly();
            _handlerTypes = GetTypesWithAttribute(assembly).ToList();

            List<string> reservedCommands = _handlerTypes.Select(tuple => "!" + tuple.Item2.CommandAlias.TrimEnd()).ToList();

            _commandMap = new CommandMap(commandRepository1);
            _commandMap.Initialize().Wait();

            _repositoryContainer = new RepositoryContainer(
                chatRepository,
                commandRepository1,
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

            var chatId = message.Chat.Id;

            if (!_messageHandlerMap.ContainsKey(chatId))
            {
                InitializeChat(chatId, client);
            }
               
            var lower = message.Text.ToLower().Trim();

            // Take the first word which will be the command, it must start with ! and end with a whitespace
            var whitespaceIndex = lower.IndexOf(' ');

            var command = lower;

            if (whitespaceIndex >= 0)
                command = lower.Substring(0, whitespaceIndex);

            // Remove the ! symbol from the beginning
            command = command.Remove(0, 1);

            var data = lower.Substring(lower.IndexOf(' ') + 1);

            var mhc = _messageHandlerMap[chatId];

            foreach (var handlerData in mhc)
            {
                if (handlerData.Metadata.CommandAlias == command)
                {
                    try
                    {
                        await handlerData.Handler.HandleAsync(message, client);
                    }
                    catch (TelegramCommandException e)
                    {
                        await client.SendTextMessageAsync(chatId, "Ошибка: " + e.Message, ParseMode.Markdown);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        await client.SendTextMessageAsync(chatId, "Непредвиденная ошибка: " + e.Message, ParseMode.Markdown);
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
                    if (c.ToLower() != "@gamee")
                        c = c.Replace("@", "");
                    await client.SendTextMessageAsync(message.Chat.Id, c);
                }
            }
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
    }
}