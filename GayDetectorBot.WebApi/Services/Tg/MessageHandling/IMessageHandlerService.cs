using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public interface IMessageHandlerService
{
    Task Update(Update? update, ITelegramBotClient client);
}