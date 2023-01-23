using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Services.Tg;

public interface ITelegramService
{
    ITelegramBotClient Client { get; }

    Task Initialize();

    Task HandleUpdateFromController(Update update, CancellationToken cancellationToken);
}