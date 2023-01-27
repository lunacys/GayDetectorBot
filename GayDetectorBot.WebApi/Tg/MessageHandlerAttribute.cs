namespace GayDetectorBot.WebApi.Tg;

public static class CommandCategories
{
    public const string Gays = "Пидорство";
    public const string DickSize = "Размер достоинства";
    public const string Commands = "Кастомные команды";
    public const string Common = "Всякое разное";
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MessageHandlerAttribute : Attribute
{
    public string CommandAlias { get; set; }
    public bool HasParameters => Parameters.Count > 0;
    public int ParameterCount => Parameters.Count;
    public List<string> Parameters { get; }

    public MessageHandlerAttribute(string commandAlias)
        : this(commandAlias, null)
    { }

    public MessageHandlerAttribute(string commandAlias, params string[]? parameters)
    {
        CommandAlias = commandAlias;
        Parameters = parameters == null ? new List<string>() : parameters.ToList();
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MessageHandlerMetadataAttribute : Attribute
{
    public string? Description { get; set; }
    public string? Category { get; set; }

    public MessageHandlerMetadataAttribute(string? description = null, string? category = null)
    {
        Description = description;
        Category = category ?? CommandCategories.Common;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class MessageHandlerPermissionAttribute : Attribute
{
    public MemberStatusPermission Permission { get; set; } =
        MemberStatusPermission.Creator | MemberStatusPermission.Administrator;

    public MessageHandlerPermissionAttribute()
    { }

    public MessageHandlerPermissionAttribute(MemberStatusPermission permissions)
    {
        Permission = permissions;
    }
}