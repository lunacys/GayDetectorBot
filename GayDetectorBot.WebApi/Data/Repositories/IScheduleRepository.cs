using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface IScheduleRepository
{
    Task SaveAsync(long chatId, string message, int messageId, DateTimeOffset fireTime);
    Task DeleteByIdAsync(int id);
    Task<IEnumerable<SchedulerContext>> RetrieveByChatId(long chatId);
    Task<IEnumerable<SchedulerContext>> RetrieveAll();
}