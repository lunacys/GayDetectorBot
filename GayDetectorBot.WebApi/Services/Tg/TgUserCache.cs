using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models.Tg;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.WebApi.Services.Tg;

public interface ITgUserCache
{
    Task NewMessage(Message message);
    Task InitializeFromDbAsync();
    Task PersistAsync();
    TgUser GetUserById(long userId);
}

public class TgUserCache : ITgUserCache
{
    private readonly Dictionary<long, TgUser> _users = new();

    private readonly ITgUserRepository _userRepository;
    readonly IChatRepository _chatRepository;
    
    public TgUserCache(ITgUserRepository userRepository, IChatRepository chatRepository)
    {
        _userRepository = userRepository;
        _chatRepository = chatRepository;
    }

    public async Task NewMessage(Message message)
    {
        if (message.From == null)
            return;

        if (!await _chatRepository.ChatExists(message.Chat.Id))
            await _chatRepository.ChatAdd(message.Chat.Id, null, null);
        
        if (_users.TryGetValue(message.From.Id, out var user))
        {
            user.TotalMessages++;
            if (IsWithContent(message))
                user.ContentSent++;
        }
        else
        {
            _users[message.From.Id] = await _userRepository.GetOrCreate(
                message.From.Id,
                message.From.Username,
                message.MessageId,
                IsWithContent(message)
            );
        }
    }
    
    public TgUser GetUserById(long userId) => _users[userId];

    public async Task InitializeFromDbAsync()
    {
        var users = await _userRepository.GetAllUsers();
        foreach (var user in users)
        {
            _users[user.UserId] = user;
        }
    }

    public async Task PersistAsync()
    {
        await _userRepository.UpdateBulk(_users.Values);
    }
    
    private bool IsWithContent(Message message)
    {
        switch (message.Type)
        {
            case MessageType.Photo:
            case MessageType.Audio:
            case MessageType.Video:
            case MessageType.Voice:
            case MessageType.Document:
                return true;
            default:
                return false;
        }
    }
}