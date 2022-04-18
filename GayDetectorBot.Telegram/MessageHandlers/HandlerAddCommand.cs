using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandlers
{
    [MessageHandler("добавить-команду", "", 2)]
    public class HandlerAddCommand : IMessageHandler
    {
        public string CommandString => "добавить-команду";
        public bool HasParameters => true;

        public MemberStatusPermission Permissions =>
            MemberStatusPermission.Administrator | MemberStatusPermission.Creator;

        private readonly CommandRepository _commandRepository;
        private readonly CommandMap _commandMap;

        public HandlerAddCommand(CommandRepository commandRepo, CommandMap commandMap)
        {
            _commandRepository = commandRepo;
            _commandMap = commandMap;
        }

        public async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var data = message.Text.Split(' ');

            var chatId = message.Chat.Id;

            if (data.Length < 3)
            {
                await client.SendTextMessageAsync(chatId, "Мало данных! Надо два параметра!");
                return;
            }

            var prefix = data[1];

            if (!prefix.StartsWith('!'))
            {
                await client.SendTextMessageAsync(chatId, "Команды должны начинаться со знака `!`", ParseMode.Markdown);
                return;
            }

            var content = data[2];

            for (int i = 3; i < data.Length; i++)
            {
                content += " " + data[i];
            }
            
            
            if (await _commandRepository.CommandExists(prefix, chatId))
            {
                await client.SendTextMessageAsync(chatId, $"Команда `{prefix}` уже существует!", ParseMode.Markdown);
            }
            else
            {
                await _commandRepository.AddCommand(chatId, message.From.Username, prefix, content);

                if (!_commandMap.ContainsKey(chatId))
                {
                    _commandMap[chatId] = new List<PrefixContent>();
                }

                _commandMap[chatId].Add(new PrefixContent
                {
                    Prefix = prefix,
                    Content = content
                });

                await client.SendTextMessageAsync(chatId, $"Команда `{prefix}` добавлена успешно", ParseMode.Markdown);
            }
        }
    }
}