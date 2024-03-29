﻿using GayDetectorBot.Telegram.Services;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers;

[MessageHandler("таймер", "добавить таймер на нужное время с напоминалкой. Формат времени: hh:mm[:ss]", MemberStatusPermission.All, "через_сколько",
    "напоминалка")]
public class HandlerSchedule : HandlerBase<string, string>
{
    public HandlerSchedule(RepositoryContainer repositoryContainer)
        : base(repositoryContainer)
    {
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

        Scheduler.Schedule(result, c, (context) => SendTextAsync($"@{message.From.Username}, напоминаю тебе:\n" + context.Message, context.MessageId));

        await SendTextAsync($"Таймер сработает через {FormatTime(result.Hours)}:{FormatTime(result.Minutes)}:{FormatTime(result.Seconds)}", msgId);
    }

    private string FormatTime(int time)
    {
        return time.ToString().PadLeft(2, '0');
    }
}