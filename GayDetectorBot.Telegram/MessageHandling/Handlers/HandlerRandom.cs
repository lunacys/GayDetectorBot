using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("рандом", "выполнить случайную команду из списка всех команд", MemberStatusPermission.All)]
    public class HandlerRandom : IMessageHandler
    {
        public string CommandString => "!рандом";

        public bool HasParameters => false;

        private readonly CommandMap _commandMap;

        public HandlerRandom(CommandMap commandMap)
        {
            _commandMap = commandMap;
        }

        public async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;

            var map = _commandMap[chatId];
            var rnd = new Random();
            var i = rnd.Next(map.Count);

            var msg = map[i].Content;
            await client.SendTextMessageAsync(chatId, msg);
        }
    }
}