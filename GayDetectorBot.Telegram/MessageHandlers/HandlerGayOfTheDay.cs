using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandlers
{
    [MessageHandler("пидордня", "стать участником рулетки", MemberStatusPermission.All)]
    public class HandlerGayOfTheDay : IMessageHandler
    {
        public string CommandString => "!пидордня";
        public MemberStatusPermission Permissions =>
            MemberStatusPermission.Administrator | MemberStatusPermission.Creator;

        public bool HasParameters => false;

        private readonly ParticipantRepository _participantRepository;

        public HandlerGayOfTheDay(ParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }

        public async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;
            var from = message?.From;

            if (from == null || from.Username == null)
                return;

            if (await _participantRepository.IsStartedForUser(from.Username, chatId))
            {
                await client.SendTextMessageAsync(chatId, $"Ты итак в деле, @{from.Username}");
            }
            else
            {
                await _participantRepository.AddUser(from.Username, chatId);

                await client.SendTextMessageAsync(chatId, $"Поздравляю, ты в деле, @{from.Username}!");
            }
        }
    }
}