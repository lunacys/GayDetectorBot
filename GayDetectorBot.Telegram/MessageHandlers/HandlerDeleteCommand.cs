using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandlers
{
    [MessageHandler("удалить-команду", "удалить кастомную команду", 1)]
    public class HandlerDeleteCommand : IMessageHandler
    {
        public string CommandString => "удалить-команду";

        public bool HasParameters => true;

        public MemberStatusPermission Permissions =>
            MemberStatusPermission.Administrator | MemberStatusPermission.Creator;

        private readonly CommandRepository _commandRepository;
        private readonly CommandMap _commandMap;

        public HandlerDeleteCommand(CommandRepository commandRepository, CommandMap commandMap)
        {
            _commandRepository = commandRepository;
            _commandMap = commandMap;
        }

        public async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            if (message.Text == null)
                return;

            var chatId = message.Chat.Id;

            var data = message.Text.Split(' ');

            if (data.Length < 2)
            {
                await client.SendTextMessageAsync(chatId, "Мало данных! Нужен один параметр!");
                return;
            }

            var prefix = data[1];

            if (!await _commandRepository.CommandExists(prefix, chatId))
            {
                await client.SendTextMessageAsync(chatId, $"Команды `{prefix}` не существует");
                return;
            }

            await _commandRepository.DeleteCommand(prefix, chatId);
            _commandMap[chatId]?.RemoveAll(pc => pc.Prefix == prefix);

            await client.SendTextMessageAsync(chatId, $"Команда `{prefix}` успешно удалена");
        }
    }
}