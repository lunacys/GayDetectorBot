namespace GayDetectorBot.WebApi.Tg;

public class HandlerMetadata
{
    public IMessageHandler Handler { get; set; } = null!;
    public MessageHandlerAttribute Metadata { get; set; } = null!;
}