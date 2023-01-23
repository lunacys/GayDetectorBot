using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Data.Repositories;

public interface ICommandRepository
{
    Task<IEnumerable<CustomCommand>> RetrieveAllCommandsByChatId(long chatId);
    Task<IEnumerable<CustomCommand>> RetrieveAllCommands();
    Task<CustomCommand?> RetrieveByPrefix(string prefix, long chatId);
    Task AddCommand(long chatId, string username, string prefix, string content);
    Task DeleteCommand(string prefix, long chatId);
    Task<bool> CommandExists(string prefix, long chatId);
}