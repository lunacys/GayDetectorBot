namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public interface IHandlerMetadataContainer
{
    IEnumerable<string> GetReservedCommands();
    IEnumerable<(Type Type, MessageHandlerAttribute Metadata)> GetHandlerTypes();
}