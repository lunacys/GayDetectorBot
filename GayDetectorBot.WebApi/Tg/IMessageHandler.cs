using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg;

public interface IMessageHandler
{
    long ChatId { get; set; }
    ITelegramBotClient Client { get; set; }

    Task HandleAsync(Message message, params string[] parsedData);
}