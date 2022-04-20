using System.Collections;
using System.Reflection;

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

    public MessageHandlerContainer(RepositoryContainer repoContainer, long chatId)
    {
        _handlers = new List<HandlerMetadata>();
        _repositoryContainer = repoContainer;
        ChatId = chatId;
    }

    public void Register(IMessageHandler handler)
    {
        var type = handler.GetType();
        var attr = type.GetCustomAttribute<MessageHandlerAttribute>(false);

        if (attr == null)
            throw new ArgumentException($"No MessageHandlerAttribute found for type {type.Name}");

        handler.ChatId = ChatId;

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