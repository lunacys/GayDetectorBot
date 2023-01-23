using Jint;

namespace GayDetectorBot.WebApi.Services.Tg.Helpers;

public class JsEvaluatorService : IJsEvaluatorService
{
    public Task<string?> EvaluateAsync(string code, Action<Engine> setupFunc)
    {
        return Task.Run(() =>
        {
            var engine = new Engine(options =>
            {
                options.LimitMemory(4_000_000);
                options.LimitRecursion(32);
                options.TimeoutInterval(TimeSpan.FromSeconds(5));
            });

            setupFunc(engine);

            code = code.Trim().Replace("```", "");
            if (code.StartsWith("`"))
                code = code.Remove(0, 1);
            if (code.EndsWith("`"))
                code = code.Remove(code.Length - 1, 1);

            Jint.Native.JsValue result;

            try
            {
                result = engine.Evaluate(code);
            }
            catch (Exception e)
            {
                return e.Message;
            }


            return result?.ToString() ?? null;
        });
    }
}