using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class PrefixContent
    {
        public string Prefix { get; set; }
        public string Content { get; set; }
    }

    public class CommandMap
    {
        private Dictionary<ulong, List<PrefixContent>> _customCommandMap;

        private readonly CommandRepository _commandRepository;

        public List<PrefixContent> this[ulong guildId]
        {
            get => _customCommandMap[guildId];
            set => _customCommandMap[guildId] = value;
        } 

        public CommandMap(CommandRepository commandRepo)
        {
            _commandRepository = commandRepo;
        }

        public async Task Initialize()
        {
            _customCommandMap = new Dictionary<ulong, List<PrefixContent>>();

            var cmds = (await _commandRepository.RetrieveAllCommands()).ToList();

            foreach (var cmd in cmds)
            {
                if (_customCommandMap.ContainsKey(cmd.GuildId))
                {
                    _customCommandMap[cmd.GuildId].Add(new PrefixContent
                        { Prefix = cmd.CommandPrefix, Content = cmd.CommandContent });
                }
                else
                {
                    _customCommandMap[cmd.GuildId] = new List<PrefixContent>();
                }
            }
        }

        public bool ContainsKey(ulong key) => _customCommandMap.ContainsKey(key);
    }
}