using System.Threading.Tasks;
using Discord.WebSocket;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerCommandList : IMessageHandler
    {
        public string CommandString => "!команды";

        public bool HasParameters => false;

        private readonly CommandMap _commandMap;

        public HandlerCommandList(CommandMap commandMap)
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

            var msg = "```";

            foreach (var pc in map)
            {
                msg += $"{pc.Prefix}\n";
            }

            msg += "```";

            await message.Channel.SendMessageAsync(msg);
        }
    }
}