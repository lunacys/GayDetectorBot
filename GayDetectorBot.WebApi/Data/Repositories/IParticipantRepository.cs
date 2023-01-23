using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface IParticipantRepository
{
    Task<bool> IsStartedForUser(string username, long chatId);
    Task AddUser(string username, long chatId, string? firstName, string? lastName);
    Task RemoveUser(long chatId, string username);
    Task<IEnumerable<Participant>> RetrieveParticipants(long chatId);
}