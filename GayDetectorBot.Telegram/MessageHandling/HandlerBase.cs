using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandling;

public abstract class HandlerBase : IMessageHandler
{
    protected RepositoryContainer RepositoryContainer { get; }

    public long ChatId { get; set; }
    public ITelegramBotClient Client { get; set; } = null!;

    public abstract Task HandleAsync(Message message, params string[] parsedData);

    protected HandlerBase(RepositoryContainer repositoryContainer)
    {
        RepositoryContainer = repositoryContainer;
    }

    public virtual async Task SendTextAsync(string message, int? messageId, ParseMode parseMode = ParseMode.Markdown)
    {
        await Client.SendTextMessageAsync(ChatId, message, parseMode, replyToMessageId: messageId);
    }

    public virtual Exception Error(string message)
    {
        return new TelegramCommandException(message);
    }

    public virtual Exception DoError(string message)
    {
        throw Error(message);
    }
}

public abstract class HandlerBase<T> : HandlerBase
{
    protected HandlerBase(RepositoryContainer repositoryContainer)
        : base(repositoryContainer)
    { }

    public abstract Task HandleAsync(Message message, string? arg);

    public override Task HandleAsync(Message message, params string[] parsedData)
    {
        var arg1 = parsedData.Length > 0 ? parsedData[0] : null;
        return HandleAsync(message, arg1);
    }
}

public abstract class HandlerBase<T1, T2> : HandlerBase
{
    protected HandlerBase(RepositoryContainer repositoryContainer)
        : base(repositoryContainer)
    { }

    public abstract Task HandleAsync(Message message, string? arg1, string? arg2);

    public override Task HandleAsync(Message message, params string[] parsedData)
    {
        var arg1 = parsedData.Length > 0 ? parsedData[0] : null;
        var arg2 = parsedData.Length > 1 ? parsedData[1] : null;

        return HandleAsync(message, arg1, arg2);
    }
}