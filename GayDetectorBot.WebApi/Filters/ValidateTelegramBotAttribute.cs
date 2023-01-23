using GayDetectorBot.WebApi.Configuration;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace GayDetectorBot.WebApi.Filters;

[AttributeUsage(AttributeTargets.Method)]
public class ValidateTelegramBotAttribute : TypeFilterAttribute
{
    public ValidateTelegramBotAttribute()
        : base(typeof(ValidateTelegramBotFilter))
    {
    }

    private class ValidateTelegramBotFilter : IActionFilter
    {
        private readonly string _secretToken;

        public ValidateTelegramBotFilter(IOptions<TelegramOptions> options)
        {
            var botConfiguration = options.Value;
            _secretToken = botConfiguration.Token;
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            if (!IsValidRequest(context.HttpContext.Request))
            {
                context.Result = new ObjectResult("\"X-Telegram-Bot-Api-Secret-Token\" is invalid")
                {
                    StatusCode = 403
                };
            }
        }

        private bool IsValidRequest(HttpRequest request)
        {
            var isSecretTokenProvided = request.Headers.TryGetValue("X-Telegram-Bot-Api-Secret-Token", out var secretTokenHeader);
            if (!isSecretTokenProvided) return false;

            return string.Equals(secretTokenHeader, _secretToken, StringComparison.Ordinal);
        }
    }
}