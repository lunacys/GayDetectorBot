using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface ITgUserRepository
{
    Task Add(long userId, string username, long totalMessages = 1, long contentSent = 0);
    Task<IEnumerable<TgUser>> GetAllUsers();
    Task<TgUser> GetOrCreate(long userId, string? username, long? messageId = null, bool isWithContent = false);
    Task UpdateBulk(IEnumerable<TgUser> users);
}

public class TgUserRepository : ITgUserRepository
{
    private readonly GayDetectorBotContext _context;

    public TgUserRepository(GayDetectorBotContext context)
    {
        _context = context;
    }

    public async Task Add(long userId, string username, long totalMessages = 1, long contentSent = 0)
    {
        await _context.TgUsers.AddAsync(new TgUser
        {
            UserId = userId,
            Username = username,
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow,
            ContentSent = contentSent,
            TotalMessages = totalMessages
        });
    }

    public async Task<IEnumerable<TgUser>> GetAllUsers()
    {
        return await _context.TgUsers.ToListAsync();
    }

    public async Task<TgUser> GetOrCreate(
        long userId,
        string? username,
        long? messageId = null,
        bool isWithContent = false
    )
    {
        var u = await _context.TgUsers
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (u != null)
            return u;

        u = new TgUser
        {
            UserId = userId,
            Username = username,
            CreatedAt = DateTime.UtcNow,
            LastActivity = DateTime.UtcNow,
            ContentSent = isWithContent ? 1 : 0,
            TotalMessages = 1,
            LastActivityMessageId = messageId
        };

        await _context.TgUsers.AddAsync(u);
        await _context.SaveChangesAsync();

        return u;
    }

    public async Task UpdateBulk(IEnumerable<TgUser> users)
    {
        _context.TgUsers.UpdateRange(users);
        await _context.SaveChangesAsync();
    }
}