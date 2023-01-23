using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Services.Tg.Helpers;

public class SchedulerService : ISchedulerService
{
    private Timer _timer = null!;

    private readonly SortedList<DateTimeOffset, (SchedulerContext, Func<SchedulerContext, Task>)> _schedules = new ();
    private readonly IScheduleRepository _scheduleRepository;

    public SchedulerService(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    public void Schedule(TimeSpan timeSpan, SchedulerContext context, Func<SchedulerContext, Task> messageAction)
    {
        var fireAt = DateTimeOffset.Now.Add(timeSpan);

        context.FireTime = fireAt;
        // TODO: Add saving to the DB
        _schedules.Add(fireAt, (context, messageAction));
    }

    public async Task Initialize()
    {
        var schedules = await _scheduleRepository.RetrieveAll();

        _timer = new Timer(async _ =>
        {
            await CheckSchedules();
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(1));
    }

    private async Task<bool> CheckSchedules()
    {
        if (_schedules.Count == 0)
            return false;

        var first = _schedules.First();
        var currTime = DateTimeOffset.Now;

        if (first.Key <= currTime)
        {
            var val = first.Value;
            await val.Item2(val.Item1);

            _schedules.RemoveAt(0);

            return true;
        }

        return false;
    }
}