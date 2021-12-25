using System.Threading.Tasks;
using Discord.WebSocket;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerParticipants : IMessageHandler
    {
        public string CommandString => "!участники";

        public bool HasParameters => false;

        private readonly ParticipantRepository _participantRepository;

        public HandlerParticipants(ParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public async Task HandleAsync(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            var pList = await _participantRepository.RetrieveParticipants(g.Id);

            string listStr = "";

            foreach (var p in pList)
            {
                var u = await message.Channel.GetUserAsync(p.UserId);
                listStr += $" - {u.Mention}\n";
            }

            await message.Channel.SendMessageAsync("Участники:\n\n" + listStr);
        }
    }
}