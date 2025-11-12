using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models.Tg;
using GayDetectorBot.WebApi.Services.Tg;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg.Handlers;

[MessageHandler("досье")]
[MessageHandlerMetadata("узнать о себе больше")]
[MessageHandlerPermission(MemberStatusPermission.All)]
public class HandlerUserData : HandlerBase
{
    private readonly ITgUserCache _cache;
    private readonly ISavedFileContainer _savedFiles;
    
    public HandlerUserData(ITgUserCache cache, ISavedFileContainer savedFiles)
    {
        _cache = cache;
        _savedFiles = savedFiles;
    }
    
    public override async Task HandleAsync(Message message, params string[] parsedData)
    {
        var userId = message.From!.Id;
        
        var tgUser = _cache.GetUserById(userId);

        var contents = "";
        
        if (tgUser.ContentSent > 0)
        {
            var contentByType = new Dictionary<SavedFileType, long>();    
            var allContent = (await _savedFiles.GetAll(message.Chat.Id)).Where(x => x.UserId == userId);

            foreach (var savedFile in allContent)
            {
                if (!contentByType.TryAdd(savedFile.Type, 1))
                    contentByType[savedFile.Type]++;
            }

            var list = new List<string>();
            foreach (var l in contentByType)
            {
                if (l.Value <= 0) continue;
                
                list.Add($"{ParseType(l.Key)}: {l.Value}");
            }
            
            contents = string.Join(", ", list);
        }

        var messageText = $"Твой логин: {tgUser.Username}\n" +
                          $"Тебя зовут: {message.From.FirstName} {message.From.LastName}\n" +
                          $"Всего зарегистрированных сообщений: {tgUser.TotalMessages}\n" +
                          $"Впервые увидел твоё сообщение: {tgUser.CreatedAt:g}\n" +
                          $"Увидел, что ты прислал что-то интересное столько раз: {tgUser.ContentSent}";

        if (!string.IsNullOrEmpty(contents))
        {
            messageText += $" ({contents})";
        }

        await SendTextAsync(messageText, message.MessageId);
    }

    private string ParseType(SavedFileType fileType)
    {
        return fileType switch
        {
            SavedFileType.Photo => "фоток",
            SavedFileType.Audio => "аудио",
            SavedFileType.Video => "видосов",
            SavedFileType.Document => "документов",
            SavedFileType.Voice => "голосовух",
            _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
        };
    }
}