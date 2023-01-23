using Jint;

namespace GayDetectorBot.WebApi.Services.Tg.Helpers;

public interface IJsEvaluatorService
{
    Task<string?> EvaluateAsync(string code, Action<Engine> setupFunc);
}