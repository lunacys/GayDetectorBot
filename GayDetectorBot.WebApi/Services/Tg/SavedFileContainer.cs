﻿using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models.Tg;
using File = System.IO.File;

namespace GayDetectorBot.WebApi.Services.Tg;

public class SavedFileContainer : ISavedFileContainer
{
    private readonly Dictionary<long, List<SavedFile>> _savedFiles;

    private readonly ISavedFileRepository _savedFileRepository;

    public SavedFileContainer(ISavedFileRepository savedFileRepository)
    {
        _savedFileRepository = savedFileRepository;
        _savedFiles = new Dictionary<long, List<SavedFile>>();
    }

    public Task Initialize()
    {
        return Task.CompletedTask;
    }

    public async Task Save(SavedFile file)
    {
        if (!_savedFiles.ContainsKey(file.ChatId))
        {
            await InitializeFromDb(file.ChatId);
        }

        _savedFiles[file.ChatId].Add(file);
        await _savedFileRepository.Save(file);
    }

    public async Task<IEnumerable<SavedFile>> GetAll(long chatId)
    {
        if (!_savedFiles.ContainsKey(chatId))
        {
            await InitializeFromDb(chatId);
        }

        return _savedFiles[chatId];
    }

    public async Task<IEnumerable<SavedFile>> GetAllByType(long chatId, SavedFileType type)
    {
        if (!_savedFiles.ContainsKey(chatId))
        {
            await InitializeFromDb(chatId);
        }

        return _savedFiles[chatId].Where(f => f.Type == type).ToList();
    }

    public async Task<SavedFile?> GetById(long chatId, string id)
    {
        if (!_savedFiles.ContainsKey(chatId))
        {
            await InitializeFromDb(chatId);
        }

        return _savedFiles[chatId].FirstOrDefault(f => f.FileId == id);
    }

    public async Task<SavedFile?> GetRandomByType(long chatId, SavedFileType type)
    {
        if (!_savedFiles.ContainsKey(chatId))
        {
            await InitializeFromDb(chatId);
        }

        var rnd = new Random();
        var files = _savedFiles[chatId].Where(f => f.Type == type).ToList();

        return files[rnd.Next(files.Count)];
    }

    private async Task InitializeFromDb(long chatId)
    {
        var files = await _savedFileRepository.RetrieveAll(chatId);
        _savedFiles[chatId] = files?.ToList() ?? new List<SavedFile>();
    }
}