using System.Threading.Tasks;
using Discord.WebSocket;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerDeleteCommand : IMessageHandler
    {
        public string CommandString => "!удалить-команду ";

        public bool HasParameters => true;

        private readonly CommandRepository _commandRepository;
        private readonly CommandMap _commandMap;

        public HandlerDeleteCommand(CommandRepository commandRepository, CommandMap commandMap)
        {
            _commandRepository = commandRepository;
            _commandMap = commandMap;
        }

        public async Task HandleAsync(SocketMessage message)
        {
            var data = message.Content.Split(' ');

            if (data.Length < 2)
            {
                await message.Channel.SendMessageAsync("Мало данных! Нужен один параметр!");
                return;
            }

            var prefix = data[1];

            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            if (!await _commandRepository.CommandExists(prefix, g.Id))
            {
                await message.Channel.SendMessageAsync($"Команды `{prefix}` не существует");
                return;
            }

            await _commandRepository.DeleteCommand(prefix, g.Id);
            _commandMap[g.Id]?.RemoveAll(pc => pc.Prefix == prefix);

            await message.Channel.SendMessageAsync($"Команда `{prefix}` успешно удалена");
        }
    }
}