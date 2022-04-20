using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("пидордня", "стать участником рулетки", MemberStatusPermission.All)]
    public class HandlerGayOfTheDay : HandlerBase
    {
        public HandlerGayOfTheDay(RepositoryContainer repositoryContainer)
            : base(repositoryContainer)
        { }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;
            var from = message?.From;

            if (from == null || from.Username == null)
                return;

            if (await RepositoryContainer.Participant.IsStartedForUser(from.Username, chatId))
            {
                await SendTextAsync($"Ты итак в деле, @{from.Username}");
            }
            else
            {
                await RepositoryContainer.Participant.AddUser(from.Username, chatId);

                await SendTextAsync($"Поздравляю, ты в деле, @{from.Username}!");
            }
        }
    }
}