using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class MessageHandler
    {
        private readonly CommandRepository _commandRepository;
        private readonly GayRepository _gayRepository;
        private readonly GuildRepository _guildRepository;
        private readonly ParticipantRepository _participantRepository;

        private List<IMessageHandler> _messageHandlers;

        private CommandMap _commandMap;

        public MessageHandler(
            CommandRepository commandRepository, 
            GayRepository gayRepository,
            GuildRepository guildRepository, 
            ParticipantRepository participantRepository
            )
        {
            _commandRepository = commandRepository;
            _gayRepository = gayRepository;
            _guildRepository = guildRepository;
            _participantRepository = participantRepository;
        }

        private async Task InitializeCustomCommands()
        {
            _commandMap = new CommandMap(_commandRepository);
            await _commandMap.Initialize();

            _messageHandlers = new List<IMessageHandler>
            {
                new HandlerAddCommand(_commandRepository, _commandMap),
                new HandlerAddParticipant(_participantRepository),
                new HandlerCommandList(_commandMap),
                new HandlerDeleteCommand(_commandRepository, _commandMap),
                new HandlerFindGay(_guildRepository, _participantRepository, _gayRepository),
                new HandlerGayOfTheDay(_participantRepository),
                new HandlerGayTop(_gayRepository),
                new HandlerHelp(),
                new HandlerParticipants(_participantRepository),
                new HandlerRandom(_commandMap),
                new HandlerRemoveMe(_participantRepository)
            };
        }

        public async Task Message(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            if (ch == null)
                return;

            var g = ch?.Guild;

            if (_commandMap == null)
            {
                await InitializeCustomCommands();
            }

            if (string.IsNullOrEmpty(message.Content))
                return;

            var lower = message.Content.ToLower().TrimEnd();

            foreach (var handler in _messageHandlers)
            {
                if (handler.HasParameters && lower.StartsWith(handler.CommandString))
                    await handler.HandleAsync(message);
                else if (handler.CommandString == lower)
                    await handler.HandleAsync(message);
            }

            if (_commandMap.ContainsKey(g.Id))
            {
                var content = _commandMap[g.Id].FirstOrDefault(pc => pc.Prefix.ToLower() == message.Content.ToLower());
                if (content != null)
                    await message.Channel.SendMessageAsync(content.Content);
            }
        }
    }
}