using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling;

public abstract class HandlerBase : IMessageHandler
{
    protected RepositoryContainer RepositoryContainer { get; }

    public long ChatId { get; set; }

    public abstract Task HandleAsync(Message message, ITelegramBotClient client);

    protected HandlerBase(RepositoryContainer repositoryContainer)
    {
        RepositoryContainer = repositoryContainer;
    }

    public virtual async Task Send(ITelegramBotClient client, long chatId, string message)
    {
        await client.SendTextMessageAsync(chatId, message);
    }
}