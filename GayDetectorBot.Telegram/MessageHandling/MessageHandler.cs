using System.Reflection;
using GayDetectorBot.Telegram.Data.Repos;
using GayDetectorBot.Telegram.MessageHandling.Handlers;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling
{
    public class MessageHandler
    {
        private readonly CommandRepository _commandRepository;
        private readonly GayRepository _gayRepository;
        private readonly ChatRepository _chatRepository;
        private readonly ParticipantRepository _participantRepository;

        private List<IMessageHandler> _messageHandlers;
        
        private CommandMap? _commandMap;

        private readonly List<string> _reservedCommands;

        public MessageHandler(
            CommandRepository commandRepository, 
            GayRepository gayRepository,
            ChatRepository chatRepository, 
            ParticipantRepository participantRepository)
        {
            _commandRepository = commandRepository;
            _gayRepository = gayRepository;
            _chatRepository = chatRepository;
            _participantRepository = participantRepository;
            _messageHandlers = new List<IMessageHandler>();

            var assembly = Assembly.GetExecutingAssembly();
            var types = GetTypesWithAttribute(assembly);

            _reservedCommands = types.Select(tuple => "!" + tuple.attribute.CommandAlias.TrimEnd()).ToList();
        }

        private async Task InitializeCustomCommands()
        {
            _commandMap = new CommandMap(_commandRepository);
            await _commandMap.Initialize();

            _messageHandlers = new List<IMessageHandler>
            {
                new HandlerAddCommand(_commandRepository, _commandMap, _reservedCommands),
                new HandlerAddParticipant(_participantRepository),
                new HandlerCommandList(_commandMap),
                new HandlerDeleteCommand(_commandRepository, _commandMap),
                new HandlerFindGay(_chatRepository, _participantRepository, _gayRepository),
                new HandlerGayOfTheDay(_participantRepository),
                new HandlerGayTop(_gayRepository),
                new HandlerHelp(),
                new HandlerParticipants(_participantRepository),
                new HandlerRandom(_commandMap),
                new HandlerRemoveMe(_participantRepository),
                new HandlerEval()
            };
        }

        public async Task Message(Message? message, ITelegramBotClient client)
        {
            if (message == null)
                return;

            if (_commandMap == null)
            {
                await InitializeCustomCommands();
            }

            if (string.IsNullOrEmpty(message.Text))
                return;

            var lower = message.Text.ToLower().TrimEnd();

            bool any = false;

            foreach (var handler in _messageHandlers)
            {
                if (handler.HasParameters && lower.StartsWith(handler.CommandString))
                {
                    await handler.HandleAsync(message, client);
                    any = true;
                }
                else if (handler.CommandString == lower)
                {
                    await handler.HandleAsync(message, client);
                    any = true;
                }
            }

            if (any)
                return;

            if (_commandMap!.ContainsKey(message.Chat.Id))
            {
                var content = _commandMap[message.Chat.Id].FirstOrDefault(prefixContent =>
                    prefixContent.Prefix.ToLower() == message.Text.ToLower());
                if (content != null)
                    await client.SendTextMessageAsync(message.Chat.Id, content.Content);
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