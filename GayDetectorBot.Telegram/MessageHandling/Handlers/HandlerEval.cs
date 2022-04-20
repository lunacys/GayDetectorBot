using Jint;
using Jint.Runtime;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers;

[MessageHandler("eval", "выполнить скрипт на JavaScript", "скрипт")]
public class HandlerEval : HandlerBase
{
    public HandlerEval(RepositoryContainer repositoryContainer)
        : base(repositoryContainer)
    { }

    public override async Task HandleAsync(Message message, ITelegramBotClient client)
    {
        var data = message.Text?.Split(" ");
        var chatId = message.Chat.Id;

        //if (jsConsole.LogAction == null)
        //    jsConsole.LogAction = (o) => client.SendTextMessageAsync(chatId, "LOG: " + o);

        if (data == null || data.Length < 2)
        {
            await client.SendTextMessageAsync(chatId, "Нет скрипта");
            return;
        }

        var engine = new Engine(options =>
        {
            options.LimitMemory(4_000_000);
            options.LimitRecursion(32);
        });

        var code = "";

        for (int i = 1; i < data.Length; i++)
        {
            code += data[i] + " ";
        }

        code = code.Trim().Replace("```", "");
        if (code.StartsWith("`"))
            code = code.Remove(0, 1);
        if (code.EndsWith("`"))
            code = code.Remove(code.Length - 1, 1);

        try
        {
            var result = engine.Evaluate(code);
            if (result != null)
                await client.SendTextMessageAsync(chatId, "Результат:\n```\n" + result + "\n```", ParseMode.Markdown);
        }
        catch (JavaScriptException e)
        {
            Console.WriteLine(e);
            await client.SendTextMessageAsync(chatId, "Ошибка выполнения скрипта:\n" + e.Message);
            return;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await client.SendTextMessageAsync(chatId, "Непредвиденная ошибка:\n" + e.Message);
            return;
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