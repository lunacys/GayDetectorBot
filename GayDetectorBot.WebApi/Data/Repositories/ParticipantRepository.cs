using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface IParticipantRepository
{
    Task<bool> IsStartedForUser(string username, long chatId);
    Task AddUser(string username, long chatId, string? firstName, string? lastName);
    Task RemoveUser(long chatId, string username);
    Task<IEnumerable<Participant>> RetrieveParticipants(long chatId);
}

public class ParticipantRepository : IParticipantRepository
{
    private readonly GayDetectorBotContext _context;

    public ParticipantRepository(GayDetectorBotContext context)
    {
        _context = context;
    }

    public async Task<bool> IsStartedForUser(string username, long chatId)
    {
        var participant =
            await _context.Participants.FirstOrDefaultAsync(p => p.ChatId == chatId && p.Username == username);

        if (participant == null)
            return false;

        return !participant.IsRemoved;
    }

    public async Task AddUser(string username, long chatId, string? firstName, string? lastName)
    {
        await _context.Participants.AddAsync(new Participant
        {
            ChatId = chatId,
            Username = username,
            FirstName = firstName,
            LastName = lastName,
            StartedAt = DateTimeOffset.Now
        });
        await _context.SaveChangesAsync();
    }

    public async Task RemoveUser(long chatId, string username)
    {
        var participant =
            await _context.Participants.FirstOrDefaultAsync(p => p.ChatId == chatId && p.Username == username);

        if (participant == null) 
            return;

        participant.IsRemoved = true;
        _context.Participants.Update(participant);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Participant>> RetrieveParticipants(long chatId)
    {
        return await _context.Participants.Where(p => p.ChatId == chatId).ToListAsync();
    }
}