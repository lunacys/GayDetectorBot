using System.Reflection;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public class HandlerMetadataContainer : IHandlerMetadataContainer
{
    private List<string> _reservedCommands = null!;
    private List<(Type type, MessageHandlerAttribute attribute)> _handlerTypes = null!;

    public HandlerMetadataContainer()
    {
        var asm = Assembly.GetExecutingAssembly();
        _handlerTypes = GetTypesWithAttribute(asm).ToList();
        _reservedCommands = _handlerTypes.Select(tuple => "!" + tuple.Item2.CommandAlias.TrimEnd()).ToList();
    }

    public IEnumerable<string> GetReservedCommands()
    {
        return _reservedCommands;
    }

    public IEnumerable<(Type Type, MessageHandlerAttribute Metadata)> GetHandlerTypes()
    {
        return _handlerTypes;
    }

    private static IEnumerable<(Type type, MessageHandlerAttribute attribute)> GetTypesWithAttribute(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attr = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);

            if (attr.Length > 0)
                yield return (type, attr[0] as MessageHandlerAttribute)!;
        }
    }
}