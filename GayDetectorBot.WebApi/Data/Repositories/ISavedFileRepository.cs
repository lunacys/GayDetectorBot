using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface ISavedFileRepository
{
    Task Save(SavedFile file);
    Task<int> GetCount();
    Task<SavedFile?> RetrieveById(int id);
    Task<IEnumerable<SavedFile>> RetrieveAll();
    Task<SavedFile?> RetrieveRandomByType(SavedFileType type);
}