using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface IChatRepository
{
    Task<bool> ChatExists(long chatId);
    Task<DateTimeOffset?> ChatLastChecked(long chatId);
    Task<string?> GetLastGay(long chatId);
    Task ChatUpdate(long chatId, string username);
    Task ChatAdd(long chatId, DateTimeOffset? dateTime, string lastGayUsername);
}

public class ChatRepository : IChatRepository
{
    private readonly GayDetectorBotContext _context;

    public ChatRepository(GayDetectorBotContext context)
    {
        _context = context;
    }

    public async Task<bool> ChatExists(long chatId)
    {
        return await _context.Chats.AnyAsync(c => c.ChatId == chatId);
    }

    public async Task<DateTimeOffset?> ChatLastChecked(long chatId)
    {
        var chat = await _context.Chats.FirstOrDefaultAsync(c => c.ChatId == chatId);

        if (chat == null)
            return null;

        return chat.LastChecked;
    }

    public async Task<string?> GetLastGay(long chatId)
    {
        var chat = await _context.Chats.FirstOrDefaultAsync(c => c.ChatId == chatId);

        if (chat == null) 
            return null;

        return chat.LastGayUsername;
    }

    public async Task ChatUpdate(long chatId, string username)
    {
        var chat = await _context.Chats.FirstOrDefaultAsync(c => c.ChatId == chatId);

        if (chat == null)
            return;

        chat.LastGayUsername = username;
        _context.Chats.Update(chat);
        await _context.SaveChangesAsync();
    }

    public async Task ChatAdd(long chatId, DateTimeOffset? dateTime, string lastGayUsername)  
    {
        await _context.Chats.AddAsync(new Chat
        {
            ChatId = chatId,
            LastChecked = dateTime,
            LastGayUsername = lastGayUsername
        });
        await _context.SaveChangesAsync();
    }
}