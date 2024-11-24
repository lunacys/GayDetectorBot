using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Tg;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling;

public interface ICommandMapService
{
    IEnumerable<string> ReservedCommands { get; }

    Task Initialize();
    bool ContainsKey(long chatId);
    List<PrefixContent> GetByChatId(long chatId);
    void SetByChatId(long chatId, IEnumerable<PrefixContent> data);

}

public class CommandMapService : ICommandMapService
{
    private readonly Dictionary<long, List<PrefixContent>> _customCommandMap;
    private readonly ICommandRepository _commandRepository;

    public IEnumerable<string> ReservedCommands { get; }

    public CommandMapService(ICommandRepository commandRepository, IHandlerMetadataContainer handlerMetadataContainer)
    {
        _commandRepository = commandRepository;

        _customCommandMap = new Dictionary<long, List<PrefixContent>>();

        ReservedCommands = handlerMetadataContainer.GetReservedCommands();
    }

    public async Task Initialize()
    {
        var cmds = (await _commandRepository.RetrieveAllCommands()).ToList();

        foreach (var cmd in cmds)
        {
            if (!_customCommandMap.ContainsKey(cmd.ChatId))
                _customCommandMap[cmd.ChatId] = new List<PrefixContent>();

            _customCommandMap[cmd.ChatId].Add(new PrefixContent
                { Prefix = cmd.CommandPrefix, Content = cmd.CommandContent });
        }
    }

    public bool ContainsKey(long chatId)
        => _customCommandMap.ContainsKey(chatId);

    public List<PrefixContent> GetByChatId(long chatId)
        => _customCommandMap.ContainsKey(chatId) ? _customCommandMap[chatId] : new List<PrefixContent>();

    public void SetByChatId(long chatId, IEnumerable<PrefixContent> data)
        => _customCommandMap[chatId] = data.ToList();
}