using Microsoft.AspNetCore.Mvc;

namespace GayDetectorBot.WebApi;

public static class Utils
{
    private static readonly string _chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_";

    public static string GenerateRandomString(int length = 16)
    {
        var stringChars = new char[length];
        var random = new Random();

        for (int i = 0; i < length; i++)
        {
            stringChars[i] = _chars[random.Next(_chars.Length)];
        }

        return new string(stringChars);
    }

    public static ControllerActionEndpointConventionBuilder MapBotWebhookRoute<T>(
        this IEndpointRouteBuilder endpoints,
        string route) where T : ControllerBase
    {
        var controllerName = typeof(T).Name.Replace("Controller", "");
        var actionName = typeof(T).GetMethods()[0].Name;

        return endpoints.MapControllerRoute(
            "bot_webhook",
            route,
            new { controller = controllerName, action = actionName }
        );
    }
}