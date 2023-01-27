using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface IScheduleRepository
{
    Task SaveAsync(long chatId, string message, int messageId, DateTimeOffset fireTime);
    Task DeleteByIdAsync(int id);
    Task<IEnumerable<SchedulerContext>> RetrieveByChatId(long chatId);
    Task<IEnumerable<SchedulerContext>> RetrieveAll();
}

public class ScheduleRepository : IScheduleRepository
{
    private readonly GayDetectorBotContext _context;

    public ScheduleRepository(GayDetectorBotContext context)
    {
        _context = context;
    }

    public async Task SaveAsync(long chatId, string message, int messageId, DateTimeOffset fireTime)
    {
        await _context.Schedules.AddAsync(new SchedulerContext
        {
            ChatId = chatId,
            Message = message,
            MessageId = messageId,
            FireTime = fireTime
        });

        await _context.SaveChangesAsync();
    }

    public async Task DeleteByIdAsync(int id)
    {
        var s = await _context.Schedules.FirstOrDefaultAsync(sch => sch.Id == id);
        if (s == null)
            return;

        _context.Remove(s);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<SchedulerContext>> RetrieveByChatId(long chatId)
    {
        return await _context.Schedules.Where(s => s.ChatId == chatId).ToListAsync();
    }

    public async Task<IEnumerable<SchedulerContext>> RetrieveAll()
    {
        return await _context.Schedules.ToListAsync();
    }
}