using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Services.Tg;

public class SavedFileContainer : ISavedFileContainer
{
    private readonly Dictionary<SavedFileType, List<SavedFile>> _savedFiles;

    private readonly ISavedFileRepository _savedFileRepository;

    public SavedFileContainer(ISavedFileRepository savedFileRepository)
    {
        _savedFileRepository = savedFileRepository;

        _savedFiles = new Dictionary<SavedFileType, List<SavedFile>>();

        var allTypes = Enum.GetValues<SavedFileType>();
        foreach (var ft in allTypes)
        {
            _savedFiles[ft] = new List<SavedFile>();
        }
    }

    public async Task Initialize()
    {
        var files = await _savedFileRepository.RetrieveAll();

        foreach (var file in files)
        {
            _savedFiles[file.Type].Add(file);
        }
    }

    public async Task Save(SavedFile file)
    {
        _savedFiles[file.Type].Add(file);
        await _savedFileRepository.Save(file);
    }

    public IEnumerable<SavedFile> GetAll()
    {
        var vals = _savedFiles.Values;

        foreach (var val in vals)
        {
            foreach (var savedFile in val)
            {
                yield return savedFile;
            }
        }
    }

    public IEnumerable<SavedFile> GetAllByType(SavedFileType type)
    {
        return _savedFiles[type];
    }

    public SavedFile? GetById(string id)
    {
        var vals = _savedFiles.Values;
        foreach (var val in vals)
        {
            foreach (var savedFile in val)
            {
                if (savedFile.FileId == id)
                    return savedFile;
            }
        }
        
        return null;
    }

    public SavedFile? GetRandomByType(SavedFileType type)
    {
        var vals = _savedFiles[type];

        var rnd = new Random();

        return vals[rnd.Next(vals.Count)];
    }
}