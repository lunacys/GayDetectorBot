using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerAddParticipant : IMessageHandler
    {
        public string CommandString => "!добавить ";

        public bool HasParameters => true;

        private readonly ParticipantRepository _participantRepository;

        public HandlerAddParticipant(ParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public async Task HandleAsync(SocketMessage message)
        {
            var data = message.Content.Split(" ");
            if (data.Length < 2)
            {
                await message.Channel.SendMessageAsync("Укажи пользователя, дурачок");
                return;
            }

            if (message.Author.Id != 140479912506032128) // loonacuse#1111
            {
                await message.Channel.SendMessageAsync("А тебе низя такое делать!");
                return;
            }

            var userRaw = data[1];
            if (string.IsNullOrEmpty(userRaw))
                return;

            ulong userId;

            if (userRaw.StartsWith("<@")) // Mention
            {
                userId = MentionUtils.ParseUser(userRaw);
            }
            else if (char.IsDigit(userRaw[0])) // User Id
            {
                userId = ulong.Parse(userRaw);
            }
            else
            {
                await message.Channel.SendMessageAsync("Какой-то неправильный пользователь");
                return;
            }

            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            if (await _participantRepository.IsStartedForUser(userId, g.Id))
            {
                await message.Channel.SendMessageAsync($"Этот парень итак в деле");
            }
            else
            {
                var user = await message.Channel.GetUserAsync(userId);

                if (user == null)
                {
                    await message.Channel.SendMessageAsync($"Не могу найти такого парня");
                    return;
                }

                await _participantRepository.AddUser(user, g.Id);

                await message.Channel.SendMessageAsync($"Поздравляю, ты в деле, {user.Mention}!");
            }
        }
    }
}