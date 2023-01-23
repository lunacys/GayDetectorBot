using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Data.Repositories;

public class CommandRepository : ICommandRepository
{
    private readonly GayDetectorBotContext _context;

    public CommandRepository(GayDetectorBotContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CustomCommand>> RetrieveAllCommandsByChatId(long chatId)
    {
        return await _context.CustomCommands.Where(c => c.ChatId == chatId).ToListAsync();
    }

    public async Task<IEnumerable<CustomCommand>> RetrieveAllCommands()
    {
        return await _context.CustomCommands.ToListAsync();
    }

    public async Task<CustomCommand?> RetrieveByPrefix(string prefix, long chatId)
    {
        return await _context.CustomCommands.FirstOrDefaultAsync(c => c.CommandPrefix == prefix);
    }

    public async Task AddCommand(long chatId, string username, string prefix, string content)
    {
        await _context.CustomCommands.AddAsync(new CustomCommand
        {
            ChatId = chatId,
            UserAddedName = username,
            CommandPrefix = prefix,
            CommandContent = content
        });
        await _context.SaveChangesAsync();
    }

    public async Task DeleteCommand(string prefix, long chatId)
    {
        var cmd = await _context.CustomCommands.FirstOrDefaultAsync(
            c => c.ChatId == chatId && c.CommandPrefix == prefix);

        if (cmd != null)
        {
            _context.CustomCommands.Remove(cmd);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> CommandExists(string prefix, long chatId)
    {
        return await _context.CustomCommands.AnyAsync(c => c.ChatId == chatId && c.CommandPrefix == prefix);
    }
}