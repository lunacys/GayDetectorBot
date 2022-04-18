namespace GayDetectorBot.Telegram.MessageHandlers;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class MessageHandlerAttribute : Attribute
{
    public string CommandAlias { get; }
    public string? Description { get; }
    public MemberStatusPermission Permission { get; }
    public bool HasParameters { get; }
    public int ParameterCount { get; }

    public MessageHandlerAttribute(string commandAlias)
        : this(commandAlias, null, MemberStatusPermission.Creator | MemberStatusPermission.Administrator)
    { }

    public MessageHandlerAttribute(string commandAlias, string? description)
        : this(commandAlias, description, 0)
    { }

    public MessageHandlerAttribute(string commandAlias, string? description = null, MemberStatusPermission permissions = MemberStatusPermission.Creator | MemberStatusPermission.Administrator)
        : this(commandAlias, description, permissions, 0)
    { }

    public MessageHandlerAttribute(string commandAlias, string? description = null, int parameterCount = 0) 
        : this(commandAlias, description, MemberStatusPermission.Creator, parameterCount)
    { }

    public MessageHandlerAttribute(string commandAlias, MemberStatusPermission permissions = MemberStatusPermission.Creator | MemberStatusPermission.Administrator, int parameterCount = 0)
        : this(commandAlias, null, permissions, parameterCount)
    { }

    public MessageHandlerAttribute(string commandAlias, string? description = null, MemberStatusPermission permissions = MemberStatusPermission.Creator | MemberStatusPermission.Administrator, int parameterCount = 0)
    {
        CommandAlias = commandAlias;
        Description = description;
        Permission = permissions;
        HasParameters = parameterCount > 0;
        ParameterCount = parameterCount;
    }
}