using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling
{
    public interface IMessageHandler
    {
        Task HandleAsync(Message message, ITelegramBotClient client);
    }
}