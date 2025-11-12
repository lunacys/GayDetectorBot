using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models.Tg;
using GayDetectorBot.WebApi.Services.Tg;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg.Handlers;

[MessageHandler("о-чате")]
[MessageHandlerMetadata("узнать побольше о чате")]
[MessageHandlerPermission(MemberStatusPermission.All)]
public class HandlerChatData : HandlerBase
{
    private readonly IChatRepository _chatRepository;
    private readonly ISavedFileContainer _savedFiles;
    
    public HandlerChatData(IChatRepository chatRepository, ISavedFileContainer savedFiles)
    {
        _chatRepository = chatRepository;
        _savedFiles = savedFiles;
    }
    
    public override async Task HandleAsync(Message message, params string[] parsedData)
    {
        var chatId = message.Chat.Id;
        
        var chat = await _chatRepository.ChatGet(chatId);
        if (chat == null)
            throw Error("Чата не существует. Што.");

        var files = await _savedFiles.GetAll(chatId);

        var contents = "";
        
        var contentByType = new Dictionary<SavedFileType, long>();
        foreach (var savedFile in files)
        {
            if (!contentByType.TryAdd(savedFile.Type, 1))
                contentByType[savedFile.Type]++;
        }
        
        if (contentByType.Any())
        {
            var list = new List<string>();
            foreach (var l in contentByType)
            {
                if (l.Value <= 0) continue;

                list.Add($"{ParseType(l.Key)}: {l.Value}");
            }

            contents = string.Join("\n", list);
        }

        var messageText = $"Чат {message.Chat.Title}\n" +
                          $"Всего понаписано сообщений: {chat.TotalMessages}";

        if (!string.IsNullOrEmpty(contents))
        {
            messageText += $", из них:\n{contents}";
        }
        
        await SendTextAsync(messageText, message.MessageId);
    }
    
    private string ParseType(SavedFileType fileType)
    {
        return fileType switch
        {
            SavedFileType.Photo => " > Фоток",
            SavedFileType.Audio => " > Аудио",
            SavedFileType.Video => " > Видосов",
            SavedFileType.Document => " > Документов",
            SavedFileType.Voice => " > Голосовух",
            _ => throw new ArgumentOutOfRangeException(nameof(fileType), fileType, null)
        };
    }
}