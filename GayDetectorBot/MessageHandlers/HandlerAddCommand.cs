using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.WebSocket;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerAddCommand : IMessageHandler
    {
        public string CommandString => "!добавить-команду ";
        public bool HasParameters => true;

        private readonly CommandRepository _commandRepository;
        private readonly CommandMap _commandMap;

        public HandlerAddCommand(CommandRepository commandRepo, CommandMap commandMap)
        {
            _commandRepository = commandRepo;
            _commandMap = commandMap;
        }

        public async Task HandleAsync(SocketMessage message)
        {
            var data = message.Content.Split(' ');

            if (data.Length < 3)
            {
                await message.Channel.SendMessageAsync("Мало данных! Надо два параметра!");
                return;
            }

            var prefix = data[1];

            if (!prefix.StartsWith('!'))
            {
                await message.Channel.SendMessageAsync("Команды должны начинаться со знака `!`");
                return;
            }

            var content = data[2];

            for (int i = 3; i < data.Length; i++)
            {
                content += " " + data[i];
            }

            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            if (await _commandRepository.CommandExists(prefix, g.Id))
            {
                await message.Channel.SendMessageAsync($"Команда `{prefix}` уже существует!");
            }
            else
            {
                await _commandRepository.AddCommand(g.Id, message.Author.Id, prefix, content);

                if (!_commandMap.ContainsKey(g.Id))
                {
                    _commandMap[g.Id] = new List<PrefixContent>();
                }

                _commandMap[g.Id].Add(new PrefixContent
                {
                    Prefix = prefix,
                    Content = content
                });

                await message.Channel.SendMessageAsync($"Команда `{prefix}` добавлена успешно");
            }
        }
    }
}