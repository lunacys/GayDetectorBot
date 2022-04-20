using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("добавить-команду", "добавить кастомную команду", "название-команды", "текстовое содержание")]
    public class HandlerAddCommand : HandlerBase
    {
        public HandlerAddCommand(RepositoryContainer repositoryContainer)
            : base(repositoryContainer)
        { }

        public override async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var data = message.Text?.Split(' ');

            var chatId = message.Chat.Id;


            if (data == null || data.Length < 3)
            {
                Error("Мало данных! Надо два параметра!");
                return;
            }

            var prefix = data[1];

            if (!prefix.StartsWith('!'))
            {
                Error("Команды должны начинаться со знака `!`");
                return;
            }

            if (RepositoryContainer.ReservedCommands.Contains(prefix))
            {
                Error("Такая команда уже занята ботом, извини");
                return;
            }

            var content = data[2];

            for (int i = 3; i < data.Length; i++)
            {
                content += " " + data[i];
            }

            if (await RepositoryContainer.Command.CommandExists(prefix, chatId))
            {
                Error($"Команда `{prefix}` уже существует!");
            }
            else
            {
                if (message.From != null && message.From.Username != null)
                    await RepositoryContainer.Command.AddCommand(chatId, message.From.Username, prefix, content);
                else
                    Error($"Неизвестный пользователь");

                if (!RepositoryContainer.CommandMap.ContainsKey(chatId))
                {
                    RepositoryContainer.CommandMap[chatId] = new List<PrefixContent>();
                }

                RepositoryContainer.CommandMap[chatId].Add(new PrefixContent
                {
                    Prefix = prefix,
                    Content = content
                });

                await SendTextAsync($"Команда `{prefix}` добавлена успешно");
            }
        }
    }
}