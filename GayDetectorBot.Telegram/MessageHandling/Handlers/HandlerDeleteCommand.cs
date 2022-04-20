using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("удалить-команду", "удалить кастомную команду", "название-команды")]
    public class HandlerDeleteCommand : HandlerBase
    {
        public HandlerDeleteCommand(RepositoryContainer repositoryContainer)
            : base(repositoryContainer) { }

        public override async Task HandleAsync(Message message, ITelegramBotClient client)
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

            if (!await RepositoryContainer.Command.CommandExists(prefix, chatId))
            {
                await client.SendTextMessageAsync(chatId, $"Команды `{prefix}` не существует");
                return;
            }

            await RepositoryContainer.Command.DeleteCommand(prefix, chatId);
            RepositoryContainer.CommandMap[chatId]?.RemoveAll(pc => pc.Prefix == prefix);

            await client.SendTextMessageAsync(chatId, $"Команда `{prefix}` успешно удалена");
        }
    }
}