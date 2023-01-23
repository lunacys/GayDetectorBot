using Telegram.Bot.Types;
using Telegram.Bot;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public interface IMessageHandler
{
    long ChatId { get; set; }
    ITelegramBotClient Client { get; set; }

    Task HandleAsync(Message message, params string[] parsedData);
}