namespace GayDetectorBot.Telegram.MessageHandling;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class MessageHandlerAttribute : Attribute
{
    public string CommandAlias { get; }
    public string? Description { get; }
    public MemberStatusPermission Permission { get; }
    public bool HasParameters => Parameters.Any();
    public int ParameterCount => Parameters.Count;
    public List<string> Parameters { get; }

    public MessageHandlerAttribute(string commandAlias)
        : this(commandAlias, null, MemberStatusPermission.Creator | MemberStatusPermission.Administrator)
    { }

    public MessageHandlerAttribute(string commandAlias, string? description)
        : this(commandAlias, description, null)
    { }

    public MessageHandlerAttribute(string commandAlias, string? description = null, MemberStatusPermission permissions = MemberStatusPermission.Creator | MemberStatusPermission.Administrator)
        : this(commandAlias, description, permissions, null)
    { }

    public MessageHandlerAttribute(string commandAlias, string? description = null, params string[]? parameters) 
        : this(commandAlias, description, MemberStatusPermission.Creator, parameters)
    { }

    public MessageHandlerAttribute(string commandAlias, MemberStatusPermission permissions = MemberStatusPermission.Creator | MemberStatusPermission.Administrator, params string[]? parameters)
        : this(commandAlias, null, permissions, parameters)
    { }

    public MessageHandlerAttribute(string commandAlias, string? description = null, MemberStatusPermission permissions = MemberStatusPermission.Creator | MemberStatusPermission.Administrator, params string[]? parameters)
    {
        CommandAlias = commandAlias;
        Description = description;
        Permission = permissions;
        Parameters = parameters == null ? new List<string>() : parameters.ToList();
    }
}