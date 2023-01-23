using GayDetectorBot.WebApi.Models.Tg;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public interface ICommandMapService
{
    IEnumerable<string> ReservedCommands { get; }

    Task Initialize();
    bool ContainsKey(long chatId);
    List<PrefixContent> GetByChatId(long chatId);
    void SetByChatId(long chatId, IEnumerable<PrefixContent> data);

}