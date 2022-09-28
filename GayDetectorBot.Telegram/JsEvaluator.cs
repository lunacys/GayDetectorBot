using GayDetectorBot.Telegram.MessageHandling;
using Jint;
using Jint.Runtime;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram;

public static class JsEvaluator
{
    public static Task<string?> EvaluateAsync(string code)
    {
        return Task.Run(() =>
        {
            var engine = new Engine(options =>
            {
                options.LimitMemory(4_000_000);
                options.LimitRecursion(32);
                options.TimeoutInterval(TimeSpan.FromSeconds(5));
            });

            code = code.Trim().Replace("```", "");
            if (code.StartsWith("`"))
                code = code.Remove(0, 1);
            if (code.EndsWith("`"))
                code = code.Remove(code.Length - 1, 1);
            
            var result = engine.Evaluate(code);
            return result?.ToString() ?? null;
        });
    }
}