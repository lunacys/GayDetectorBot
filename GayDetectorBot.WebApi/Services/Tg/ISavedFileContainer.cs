using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Services.Tg;

public interface ISavedFileContainer
{
    Task Initialize();
    Task Save(SavedFile file);

    IEnumerable<SavedFile> GetAll();
    IEnumerable<SavedFile> GetAllByType(SavedFileType type);
    SavedFile? GetById(string id);
    SavedFile? GetRandomByType(SavedFileType type);
}