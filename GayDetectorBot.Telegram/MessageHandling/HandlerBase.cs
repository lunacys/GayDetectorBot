using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandling;

public abstract class HandlerBase : IMessageHandler
{
    protected RepositoryContainer RepositoryContainer { get; }

    public long ChatId { get; set; }
    public ITelegramBotClient Client { get; set; } = null!;

    public abstract Task HandleAsync(Message message, ITelegramBotClient client);

    protected HandlerBase(RepositoryContainer repositoryContainer)
    {
        RepositoryContainer = repositoryContainer;
    }

    public virtual async Task SendTextAsync(string message, ParseMode parseMode = ParseMode.Markdown)
    {
        await Client.SendTextMessageAsync(ChatId, message, parseMode);
    }

    public virtual void Error(string message)
    {
        throw new TelegramCommandException(message);
    }
}