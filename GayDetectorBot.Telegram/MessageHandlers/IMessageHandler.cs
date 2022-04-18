using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandlers
{
    public interface IMessageHandler
    {
        public string CommandString { get; }
        public bool HasParameters { get; }
        Task HandleAsync(Message message, ITelegramBotClient client);
    }
}