using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Services.Tg;

public interface ISavedFileContainer
{
    Task Initialize();
    Task Save(SavedFile file);

    Task<IEnumerable<SavedFile>> GetAll(long chatId);
    Task<IEnumerable<SavedFile>> GetAllByType(long chatId, SavedFileType type);
    Task<SavedFile?> GetById(long chatId, string id);
    Task<SavedFile?> GetRandomByType(long chatId, SavedFileType type);
}