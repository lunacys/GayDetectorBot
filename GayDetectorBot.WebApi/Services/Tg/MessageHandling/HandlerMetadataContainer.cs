using System.Reflection;
using GayDetectorBot.WebApi.Tg;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public interface IHandlerMetadataContainer
{
    IEnumerable<string> GetReservedCommands();
    IEnumerable<(Type Type, HandlerMetadata Metadata)> GetHandlerTypes();
}

public class HandlerMetadataContainer : IHandlerMetadataContainer
{
    private readonly List<string> _reservedCommands;
    private readonly List<(Type type, HandlerMetadata metadata)> _handlerTypes;

    public HandlerMetadataContainer()
    {
        var asm = Assembly.GetExecutingAssembly();
        _handlerTypes = GetTypesWithAttribute(asm).ToList();
        _reservedCommands = _handlerTypes.Select(tuple => "!" + tuple.metadata.Common.CommandAlias.TrimEnd()).ToList();
    }

    public IEnumerable<string> GetReservedCommands()
    {
        return _reservedCommands;
    }

    public IEnumerable<(Type Type, HandlerMetadata Metadata)> GetHandlerTypes()
    {
        return _handlerTypes;
    }

    public static IEnumerable<(Type type, HandlerMetadata metadata)> GetTypesWithAttribute(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attr = type.GetCustomAttribute<MessageHandlerAttribute>(false);

            if (attr != null)
            {
                var meta = type.GetCustomAttribute<MessageHandlerMetadataAttribute>(false);
                var perm = type.GetCustomAttribute<MessageHandlerPermissionAttribute>(false);

                yield return (type, new HandlerMetadata
                {
                    Common = attr,
                    Metadata = meta,
                    Permission = perm
                })!;
            }
        }
    }
}