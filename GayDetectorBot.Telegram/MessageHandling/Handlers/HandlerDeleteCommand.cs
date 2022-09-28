using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("удалить-команду", "удалить кастомную команду", "название-команды")]
    public class HandlerDeleteCommand : HandlerBase<string>
    {
        public HandlerDeleteCommand(RepositoryContainer repositoryContainer)
            : base(repositoryContainer) { }

        public override async Task HandleAsync(Message message, string? prefix)
        {
            var chatId = message.Chat.Id;

            if (prefix == null)
            {
                throw Error("Мало данных! Нужен один параметр!");
            }

            if (!await RepositoryContainer.Command.CommandExists(prefix, chatId))
            {
                throw Error($"Команды `{prefix}` не существует");
            }

            await RepositoryContainer.Command.DeleteCommand(prefix, chatId);
            RepositoryContainer.CommandMap[chatId]?.RemoveAll(pc => pc.Prefix == prefix);

            await SendTextAsync($"Команда `{prefix}` успешно удалена", message.MessageId);
        }
    }
}