using Microsoft.OpenApi.Models;

namespace GayDetectorBot.WebApi.Configuration;

public class AppSettings
{
    public string Secret { get; init; } = null!;
    public string DefaultUserName { get; init; } = null!;
    public string DefaultUserEmail { get; init; } = null!;
    public string DefaultUserPasswordHash { get; init; } = null!;
}

public static class AppVersionInfo
{
    public static readonly string AppName = "GayDetectorBot.WebApi";
    public static readonly string BuildVersion = "v.0.1";

    public static readonly OpenApiInfo OpenApiInfo = new OpenApiInfo
    {
        Version = BuildVersion,
        Title = "GayDetectorBot Web API"
    };
}