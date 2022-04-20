using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("рандом", "выполнить случайную команду из списка всех команд", MemberStatusPermission.All)]
    public class HandlerRandom : HandlerBase
    {
        public HandlerRandom(RepositoryContainer repositoryContainer)
            : base(repositoryContainer)
        { }

        public override async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;

            var map = RepositoryContainer.CommandMap[chatId];
            var rnd = new Random();
            var i = rnd.Next(map.Count);

            var msg = map[i].Content;
            await client.SendTextMessageAsync(chatId, msg);
        }
    }
}