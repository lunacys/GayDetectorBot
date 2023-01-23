using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface IGayRepository
{
    Task AddGay(Participant participant);
    Task<IEnumerable<Gay>> RetrieveGays(long chatId);

}