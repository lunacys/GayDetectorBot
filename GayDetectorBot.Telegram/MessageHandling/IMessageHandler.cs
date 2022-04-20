using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling
{
    public interface IMessageHandler
    {
        long ChatId { get; set; }

        Task HandleAsync(Message message, ITelegramBotClient client);
    }
}