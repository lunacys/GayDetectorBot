using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("участники", "список всех участников", MemberStatusPermission.All)]
    public class HandlerParticipants : IMessageHandler
    {
        public string CommandString => "!участники";

        public bool HasParameters => false;

        private readonly ParticipantRepository _participantRepository;

        public HandlerParticipants(ParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;

            var pList = await _participantRepository.RetrieveParticipants(chatId);

            string listStr = "";

            foreach (var p in pList)
            {
                listStr += $" - @{p.Username}\n";
            }

            await client.SendTextMessageAsync(chatId, "Участники:\n\n" + listStr);
        }
    }
}