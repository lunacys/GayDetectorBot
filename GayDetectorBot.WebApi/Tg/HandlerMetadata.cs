namespace GayDetectorBot.WebApi.Tg;

public class HandlerMetadata
{
    public IMessageHandler Handler { get; set; } = null!;
    public MessageHandlerAttribute Common { get; set; } = null!;
    public MessageHandlerMetadataAttribute? Metadata { get; set; }
    public MessageHandlerPermissionAttribute? Permission { get; set; }
}