using System.Collections;

namespace GayDetectorBot.Telegram.Services;

public static class Scheduler
{
    private static Timer _timer = null!;

    private static bool _initialized = false;

    private static readonly SortedList<DateTimeOffset, (SchedulerContext, Func<SchedulerContext, Task>)> _schedules = new();

    public static void Initialize()
    {
        if (_initialized)
            return;

        _timer = new Timer(state =>
        {
            CheckSchedules().Wait();
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));

        _initialized = true;
    }

    public static void Dispose()
    {
        _timer.Dispose();
    }

    public static void Schedule(TimeSpan timeSpan, SchedulerContext context, Func<SchedulerContext, Task> messageAction)
    {
        var fireAt = DateTimeOffset.Now.Add(timeSpan);

        _schedules.Add(fireAt, (context, messageAction));
    }

    private static async Task CheckSchedules()
    {
        if (_schedules.Count == 0)
            return;

        var first = _schedules.First();
        var currTime = DateTimeOffset.Now;

        if (first.Key <= currTime)
        {
            var val = first.Value;
            await val.Item2(val.Item1);

            _schedules.RemoveAt(0);
        }
    }
}