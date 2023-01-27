using GayDetectorBot.WebApi.Services.Tg.Helpers;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Jint.Runtime;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.WebApi.Tg.Handlers;

[MessageHandler("eval", "выполнить скрипт на JavaScript", "скрипт")]
public class HandlerEval : HandlerBase<string>
{
    private readonly IJsEvaluatorService _jsEvaluatorService;
    private readonly ICommandMapService _commandMapService;

    public HandlerEval(IJsEvaluatorService jsEvaluatorService, ICommandMapService commandMapService)
    {
        _jsEvaluatorService = jsEvaluatorService;
        _commandMapService = commandMapService;
    }

    public override async Task HandleAsync(Message message, string? code)
    {
        //if (jsConsole.LogAction == null)
        //    jsConsole.LogAction = (o) => client.SendTextMessageAsync(chatId, "LOG: " + o);

        if (code == null)
        {
            throw Error("Нет скрипта");
        }

        try
        {
            var result = await _jsEvaluatorService.EvaluateAsync(code, engine =>
            {
                engine.SetValue("SendCommand", async (string command) =>
                    {
                        var cmd = _commandMapService.GetByChatId(ChatId).Find(content => content.Prefix == command);
                        if (cmd != null)
                            await SendTextAsync(cmd.Content, null, ParseMode.Html);
                    }
                );
            });

            if (result != null)
                await SendTextAsync("Результат:\n```\n" + result + "\n```", message.MessageId);
        }
        catch (JavaScriptException e)
        {
            Console.WriteLine(e);
            await SendTextAsync("Ошибка выполнения скрипта:\n" + e.Message, message.MessageId);
        }
        catch (TimeoutException e)
        {
            Console.WriteLine(e);
            await SendTextAsync("Словил таймаут, скрипт выполнялся слишком долго", message.MessageId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await SendTextAsync("Непредвиденная ошибка:\n" + e.Message, message.MessageId);
        }
    }

    class JsConsole
    {
        public Action<object>? LogAction { get; set; }

        public void log(params object[] objects)
        {
            foreach (var o in objects)
            {
                Console.WriteLine(o);
                if (LogAction != null)
                {
                    LogAction(o);
                }
            }
        }

        public void debug(params object[] objects)
        {
            foreach (var o in objects)
            {
                Console.WriteLine(o);
                if (LogAction != null)
                {
                    LogAction(o);
                }
            }
        }

        public void warn(params object[] objects)
        {
            foreach (var o in objects)
            {
                Console.WriteLine(o);
                if (LogAction != null)
                {
                    LogAction(o);
                }
            }
        }

        public void error(params object[] objects)
        {
            foreach (var o in objects)
            {
                Console.WriteLine(o);
                if (LogAction != null)
                {
                    LogAction(o);
                }
            }
        }

        public void clear()
        {
            Console.Clear();
        }

        public void assert(bool condition, params object[] objects)
        {
            if (!condition)
            {
                error(objects);
            }
        }
    }
}