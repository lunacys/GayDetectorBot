using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandlers
{
    [MessageHandler("уберименя", "убрать из списка рулетки - команда только для настоящих пидоров", MemberStatusPermission.All)]
    public class HandlerRemoveMe : IMessageHandler
    {
        public string CommandString => "!уберименя";

        public bool HasParameters => false;

        private readonly ParticipantRepository _participantRepository;

        public HandlerRemoveMe(ParticipantRepository participantRepository)
        {
            _participantRepository = participantRepository;
        }
        public async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var username = message.From?.Username;

            if (username == null)
                return;

            await _participantRepository.RemoveUser(message.Chat.Id, username);

            await client.SendTextMessageAsync(message.Chat.Id, $"Ну ты и пидор, @{message.From.Username}. Убрал тебя.");
        }
    }
}