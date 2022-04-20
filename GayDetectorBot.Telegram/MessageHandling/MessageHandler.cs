using System.Reflection;
using GayDetectorBot.Telegram.Data.Repos;
using GayDetectorBot.Telegram.MessageHandling.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling
{
    public class MessageHandler
    {
        private readonly CommandMap? _commandMap;
        private readonly MessageHandlerContainer _messageHandlerContainer;

        public MessageHandler(
            CommandRepository commandRepository, 
            GayRepository gayRepository,
            ChatRepository chatRepository, 
            ParticipantRepository participantRepository)
        {
            var commandRepository1 = commandRepository;

            var assembly = Assembly.GetExecutingAssembly();
            var types = GetTypesWithAttribute(assembly).ToList();

            List<string> reservedCommands = types.Select(tuple => "!" + tuple.attribute.CommandAlias.TrimEnd()).ToList();

            _commandMap = new CommandMap(commandRepository1);
            _commandMap.Initialize().Wait();

            var repositoryContainer = new RepositoryContainer(
                chatRepository,
                commandRepository1,
                gayRepository,
                participantRepository,
                _commandMap,
                reservedCommands
            );

            _messageHandlerContainer = new MessageHandlerContainer(repositoryContainer);

            foreach (var valueTuple in types)
            {
                var mh = Activator.CreateInstance(valueTuple.type, repositoryContainer) as IMessageHandler;
                if (mh == null)
                    throw new Exception("Could not create an instance");

                _messageHandlerContainer.Register(mh);
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
               
            var lower = message.Text.ToLower().Trim();

            // Take the first word which will be the command, it must start with ! and end with a whitespace
            var whitespaceIndex = lower.IndexOf(' ');

            var command = lower;

            if (whitespaceIndex >= 0)
                command = lower.Substring(0, whitespaceIndex);

            // Remove the ! symbol from the beginning
            command = command.Remove(0, 1);

            var data = lower.Substring(lower.IndexOf(' ') + 1);

            foreach (var handlerData in _messageHandlerContainer)
            {
                if (handlerData.Metadata.CommandAlias == command)
                {
                    await handlerData.Handler.HandleAsync(message, client);
                    return;
                }
            }
            
            // Checking if command is a custom command
            if (_commandMap!.ContainsKey(message.Chat.Id))
            {
                var content = _commandMap[message.Chat.Id].FirstOrDefault(prefixContent =>
                    prefixContent.Prefix.ToLower() == message.Text.ToLower());
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