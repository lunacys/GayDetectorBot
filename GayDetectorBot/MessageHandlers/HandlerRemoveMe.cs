using System.Threading.Tasks;
using Discord.WebSocket;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerRemoveMe : IMessageHandler
    {
        public string CommandString => "!уберименя";

        public bool HasParameters => false;

        private readonly ParticipantRepository _participantRepository;

        public HandlerRemoveMe(ParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public async Task HandleAsync(SocketMessage message)
        {
            var userId = message.Author.Id;
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            await _participantRepository.RemoveUser(g.Id, userId);

            await message.Channel.SendMessageAsync($"Ну ты и пидор, {message.Author.Mention}. Убрал тебя.");
        }
    }
}