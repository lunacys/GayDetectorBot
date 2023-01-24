using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Data.Repositories;

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

    public async Task<int> GetCount()
    {
        return await _context.SavedFiles.CountAsync();
    }

    public async Task<SavedFile?> RetrieveById(int id)
    {
        return await _context.SavedFiles.FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<IEnumerable<SavedFile>> RetrieveAll()
    {
        return await _context.SavedFiles.ToListAsync();
    }

    public async Task<SavedFile?> RetrieveRandomByType(SavedFileType type)
    {
        var files = await _context.SavedFiles.Where(f => f.Type == type).ToListAsync();

        if (files.Count == 0)
            return null;

        var rnd = new Random();

        return files[rnd.Next(0, files.Count)];
    }
}