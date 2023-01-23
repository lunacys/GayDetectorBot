using Telegram.Bot;

namespace GayDetectorBot.WebApi.Services.Tg;

public interface ITelegramService
{
    ITelegramBotClient Client { get; }

    Task Initialize();
}