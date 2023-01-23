namespace GayDetectorBot.WebApi.Data.Repositories;

public interface IChatRepository
{
    Task<bool> ChatExists(long chatId);
    Task<DateTimeOffset?> ChatLastChecked(long chatId);
    Task<string?> GetLastGay(long chatId);
    Task ChatUpdate(long chatId, string username);
    Task ChatAdd(long chatId, DateTimeOffset? dateTime, string lastGayUsername);
}