using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("участники", "список всех участников", MemberStatusPermission.All)]
    public class HandlerParticipants : HandlerBase
    {
        public HandlerParticipants(RepositoryContainer repositoryContainer)
            : base(repositoryContainer)
        { }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;

            var pList = (await RepositoryContainer.Participant.RetrieveParticipants(chatId)).ToList();

            if (pList.Count == 0)
                throw Error("Нет ни одного участника");

            string listStr = "";

            foreach (var p in pList)
            {
                listStr += $" - @{p.Username}\n";
            }

            await SendTextAsync("Участники:\n\n" + listStr);
        }
    }
}