using System.Collections;
using System.Reflection;
using Telegram.Bot;

namespace GayDetectorBot.Telegram.MessageHandling;

public class HandlerMetadata
{
    public IMessageHandler Handler { get; set; } = null!;
    public MessageHandlerAttribute Metadata { get; set; } = null!;
}

public class MessageHandlerContainer : IEnumerable<HandlerMetadata>
{
    private readonly List<HandlerMetadata> _handlers;
    private readonly RepositoryContainer _repositoryContainer;

    public long ChatId { get; }
    public ITelegramBotClient Client { get; }

    public MessageHandlerContainer(RepositoryContainer repoContainer, long chatId, ITelegramBotClient client)
    {
        _handlers = new List<HandlerMetadata>();
        _repositoryContainer = repoContainer;
        ChatId = chatId;
        Client = client;
    }

    public void Register(IMessageHandler handler)
    {
        var type = handler.GetType();
        var attr = type.GetCustomAttribute<MessageHandlerAttribute>(false);

        if (attr == null)
            throw new ArgumentException($"No MessageHandlerAttribute found for type {type.Name}");

        handler.ChatId = ChatId;
        handler.Client = Client;

        _handlers.Add(new HandlerMetadata
        {
            Handler = handler,
            Metadata = attr
        });
    }

    public IEnumerator<HandlerMetadata> GetEnumerator()
    {
        return _handlers.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}