using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerGayOfTheDay : IMessageHandler
    {
        public string CommandString => "!пидордня";

        public bool HasParameters => false;

        private readonly ParticipantRepository _participantRepository;

        public HandlerGayOfTheDay(ParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public async Task HandleAsync(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;
            var userId = message.Author.Id;

            if (await _participantRepository.IsStartedForUser(userId, g.Id))
            {
                await message.Channel.SendMessageAsync($"Ты итак в деле");
            }
            else
            {
                var user = await message.Channel.GetUserAsync(userId);

                await _participantRepository.AddUser(user, g.Id);

                await message.Channel.SendMessageAsync($"Поздравляю, ты в деле, {user.Mention}!");
            }
        }
    }
}