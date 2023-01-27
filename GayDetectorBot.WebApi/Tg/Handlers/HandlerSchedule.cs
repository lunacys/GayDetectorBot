using GayDetectorBot.WebApi.Models.Tg;
using GayDetectorBot.WebApi.Services.Tg.Helpers;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg.Handlers;

[MessageHandler("таймер", "через_сколько", "напоминалка")]
[MessageHandlerMetadata("добавить таймер на нужное время с напоминалкой. Формат времени: hh:mm[:ss]")]
[MessageHandlerPermission(MemberStatusPermission.All)]
public class HandlerSchedule : HandlerBase<string, string>
{
    private readonly ISchedulerService _schedulerService;

    public HandlerSchedule(ISchedulerService schedulerService)
    {
        _schedulerService = schedulerService;
    }

    public override async Task HandleAsync(Message message, string? arg1, string? arg2)
    {
        var chatId = message.Chat.Id;
        var msgId = message.MessageId;

        if (arg1 == null)
            throw Error("Не указано время!");
        if (arg2 == null)
            throw Error("Не указана напоминалка!");

        if (!arg1.Contains(':'))
            throw Error("Неправильный формат времени. Указывай как `hh:mm[:ss]` (секунды необязательно)");

        var success = TimeSpan.TryParse(arg1, out var result);

        if (!success)
            throw Error("Не получилось распарсить время");

        var c = new SchedulerContext
        {
            Message = arg2,
            MessageId = msgId,
            ChatId = chatId
        };

        _schedulerService.Schedule(result, c, (context) => SendTextAsync($"@{message.From.Username}, напоминаю тебе:\n" + context.Message, context.MessageId));

        await SendTextAsync($"Таймер сработает через {FormatTime(result.Hours)}:{FormatTime(result.Minutes)}:{FormatTime(result.Seconds)}", msgId);
    }

    private string FormatTime(int time)
    {
        return time.ToString().PadLeft(2, '0');
    }
}