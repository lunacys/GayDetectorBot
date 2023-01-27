using GayDetectorBot.WebApi.Data.Repositories;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg.Handlers.GayHandling
{
    [MessageHandler("участники")]
    [MessageHandlerMetadata("список всех участников")]
    [MessageHandlerPermission(MemberStatusPermission.All)]
    public class HandlerParticipants : HandlerBase
    {
        private readonly IParticipantRepository _participantRepository;

        public HandlerParticipants(IParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;

            var pList = (await _participantRepository.RetrieveParticipants(chatId)).ToList();

            if (pList.Count == 0)
                throw Error("Нет ни одного участника");

            string listStr = "";

            foreach (var p in pList)
            {
                listStr += $" - @{p.Username}";
                if (p.IsRemoved)
                    listStr += " - решил уйти от обязательств";
                listStr += "\n";
            }

            await SendTextAsync("Участники:\n\n" + listStr, message.MessageId);
        }
    }
}