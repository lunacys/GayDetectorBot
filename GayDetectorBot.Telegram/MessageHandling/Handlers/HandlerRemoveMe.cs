using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("уберименя", "убрать из списка рулетки - команда только для настоящих пидоров", MemberStatusPermission.All)]
    public class HandlerRemoveMe : HandlerBase
    {
        public HandlerRemoveMe(RepositoryContainer repositoryContainer)
            : base(repositoryContainer)
        { }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var username = message.From?.Username;

            if (username == null)
                return;

            await RepositoryContainer.Participant.RemoveUser(message.Chat.Id, username);

            if (message.From == null)
            {
                throw Error($"Неизвестный пользователь");
            }
            
            await SendTextAsync($"Ну ты и пидор, @{message.From.Username}. Убрал тебя.", message.MessageId);
        }
    }
}