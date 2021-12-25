using System;
using System.Threading.Tasks;
using Discord.WebSocket;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerRandom : IMessageHandler
    {
        public string CommandString => "!рандом";

        public bool HasParameters => false;

        private readonly CommandMap _commandMap;

        public HandlerRandom(CommandMap commandMap)
        {
            _commandMap = commandMap;
        }

        public async Task HandleAsync(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            if (ch == null)
                return;

            var g = ch?.Guild;

            var map = _commandMap[g.Id];
            var rnd = new Random();
            var i = rnd.Next(map.Count);

            var msg = map[i].Content;
            await message.Channel.SendMessageAsync(msg);
        }
    }
}