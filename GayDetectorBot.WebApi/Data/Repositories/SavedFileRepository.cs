using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface ISavedFileRepository
{
    Task Save(SavedFile file);
    Task<int> GetCount(long chatId);
    Task<SavedFile?> RetrieveById(long chatId, int id);
    Task<IEnumerable<SavedFile>> RetrieveAll(long chatId);
}

public class SavedFileRepository : ISavedFileRepository
{
    private readonly GayDetectorBotContext _context;

    public SavedFileRepository(GayDetectorBotContext context)
    {
        _context = context;
    }

    public async Task Save(SavedFile file)
    {
        await _context.SavedFiles.AddAsync(file);
        await _context.SaveChangesAsync();
    }

    public async Task<int> GetCount(long chatId)
    {
        return await _context.SavedFiles.Where(f => f.ChatId == chatId).CountAsync();
    }

    public async Task<SavedFile?> RetrieveById(long chatId, int id)
    {
        return await _context.SavedFiles.FirstOrDefaultAsync(f => f.ChatId == chatId &&  f.Id == id);
    }

    public async Task<IEnumerable<SavedFile>> RetrieveAll(long chatId)
    {
        return await _context.SavedFiles.Where(f => f.ChatId == chatId).ToListAsync();
    }
}