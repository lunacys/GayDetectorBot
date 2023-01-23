using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Services.Tg.Helpers;

public interface ISchedulerService
{
    Task Initialize();
    void Schedule(TimeSpan timeSpan, SchedulerContext context, Func<SchedulerContext, Task> messageAction);
}