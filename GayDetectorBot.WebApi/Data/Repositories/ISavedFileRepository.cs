using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface ISavedFileRepository
{
    Task Save(SavedFile file);
    Task<int> GetCount(long chatId);
    Task<SavedFile?> RetrieveById(long chatId, int id);
    Task<IEnumerable<SavedFile>> RetrieveAll(long chatId);
}