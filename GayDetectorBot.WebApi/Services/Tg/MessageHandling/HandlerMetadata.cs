namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public class HandlerMetadata
{
    public IMessageHandler Handler { get; set; } = null!;
    public MessageHandlerAttribute Metadata { get; set; } = null!;
}