using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi.Data.Repositories;

public class GayRepository : IGayRepository
{
    private readonly GayDetectorBotContext _context;

    public GayRepository(GayDetectorBotContext context)
    {
        _context = context;
    }

    public async Task AddGay(Participant participant)
    {
        await _context.Gays.AddAsync(new Gay
        {
            Participant = participant,
            ParticipantId = participant.Id,
            DateTimestamp = DateTimeOffset.Now
        });

        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Gay>> RetrieveGays(long chatId)
    {
        return await _context.Gays
            .Include(g => g.Participant)
            .Where(g => g.Participant.ChatId == chatId)
            .ToListAsync();
    }
}