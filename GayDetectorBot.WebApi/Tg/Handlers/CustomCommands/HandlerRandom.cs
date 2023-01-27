using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.WebApi.Tg.Handlers.CustomCommands
{
    [MessageHandler("рандом")]
    [MessageHandlerMetadata("выполнить случайную команду из списка всех команд")]
    [MessageHandlerPermission(MemberStatusPermission.All)]
    public class HandlerRandom : HandlerBase
    {
        private readonly ICommandMapService _commandMapService;

        public HandlerRandom(ICommandMapService commandMapService)
        {
            _commandMapService = commandMapService;
        }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;

            var map = _commandMapService.GetByChatId(chatId);

            if (map.Count == 0)
            {
                throw Error("Нету ни одной пользовательской команды");
            }

            var rnd = new Random();
            var i = rnd.Next(map.Count);

            var msg = map[i];
            await SendTextAsync(msg.Content, message.MessageId, ParseMode.Html);
        }
    }
}