using GayDetectorBot.Telegram.Data.Repos;

namespace GayDetectorBot.Telegram.MessageHandlers
{
    public class PrefixContent
    {
        public string Prefix { get; set; }
        public string Content { get; set; }
    }

    public class CommandMap
    {
        private Dictionary<long, List<PrefixContent>> _customCommandMap;

        private readonly CommandRepository _commandRepository;

        public List<PrefixContent> this[long chatId]
        {
            get => _customCommandMap.ContainsKey(chatId) ? _customCommandMap[chatId] : new List<PrefixContent>();
            set => _customCommandMap[chatId] = value;
        } 

        public CommandMap(CommandRepository commandRepo)
        {
            _commandRepository = commandRepo;
        }

        public async Task Initialize()
        {
            _customCommandMap = new Dictionary<long, List<PrefixContent>>();

            var cmds = (await _commandRepository.RetrieveAllCommands()).ToList();

            foreach (var cmd in cmds)
            {
                if (_customCommandMap.ContainsKey(cmd.ChatId))
                {
                    _customCommandMap[cmd.ChatId].Add(new PrefixContent
                        { Prefix = cmd.CommandPrefix, Content = cmd.CommandContent });
                }
                else
                {
                    _customCommandMap[cmd.ChatId] = new List<PrefixContent>();
                }
            }
        }

        public bool ContainsKey(long key) => _customCommandMap.ContainsKey(key);
    }
}