using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface ITgUserChatLinkRepository
{
    Task AddUserToChatAsync(long userId, long chatId);
    /// <summary>
    /// Returns false if user exists in the chat. True otherwise.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="chatId"></param>
    /// <returns>False is user exists in the chat. True otherwise.</returns>
    Task<bool> AddIfNotExistsUserInChatAsync(long userId, long chatId);
    Task RemoveUserFromChatAsync(long userId, long chatId);
    Task<IEnumerable<TgUserChatLink>> GetAllUsersInChat(long chatId);
    Task<IEnumerable<TgUserChatLink>> GetChatsByUserId(long userId);
    Task<TgUserChatLink?> Get(long userId, long chatId);
}

public class TgUserChatLinkRepository : ITgUserChatLinkRepository 
{
    private readonly GayDetectorBotContext _context;

    public TgUserChatLinkRepository(GayDetectorBotContext context)
    {
        _context = context;
    }

    public async Task AddUserToChatAsync(long userId, long chatId)
    {
        await _context.TgUserChatLinks.AddAsync(new TgUserChatLink
        {
            UserId = userId,
            ChatId = chatId,
            CreatedAt = DateTime.UtcNow
        });
        
        await _context.SaveChangesAsync();
    }

    public async Task<bool> AddIfNotExistsUserInChatAsync(long userId, long chatId)
    {
        var link = await _context.TgUserChatLinks
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ChatId == chatId);

        if (link != null)
            return false;
        
        await AddUserToChatAsync(userId, chatId);
        return true;
    }

    public async Task RemoveUserFromChatAsync(long userId, long chatId)
    {
        var link = await _context.TgUserChatLinks
            .FirstOrDefaultAsync(l => l.UserId == userId && l.ChatId == chatId);

        if (link == null)
            return;
        
        link.IsRemoved = true;
        _context.TgUserChatLinks.Update(link);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<TgUserChatLink>> GetAllUsersInChat(long chatId)
    {
        return await _context.TgUserChatLinks.Where(l => l.ChatId == chatId).ToListAsync(); 
    }

    public async Task<IEnumerable<TgUserChatLink>> GetChatsByUserId(long userId)
    {
        return await _context.TgUserChatLinks.Where(l => l.UserId == userId).ToListAsync();
    }

    public async Task<TgUserChatLink?> Get(long userId, long chatId)
    {
        return await _context.TgUserChatLinks.FirstOrDefaultAsync(l => l.UserId == userId && l.ChatId == chatId);
    }
}