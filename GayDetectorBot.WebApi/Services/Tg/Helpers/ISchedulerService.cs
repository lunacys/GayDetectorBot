using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Services.Tg.Helpers;

public interface ISchedulerService
{
    void Schedule(TimeSpan timeSpan, SchedulerContext context, Func<SchedulerContext, Task> messageAction);
}