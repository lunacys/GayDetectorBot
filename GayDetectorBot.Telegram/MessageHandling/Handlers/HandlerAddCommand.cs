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
                await Send(client, chatId, "Мало данных! Надо два параметра!");
                return;
            }

            var prefix = data[1];

            if (!prefix.StartsWith('!'))
            {
                await client.SendTextMessageAsync(chatId, "Команды должны начинаться со знака `!`", ParseMode.Markdown);
                return;
            }

            if (RepositoryContainer.ReservedCommands.Contains(prefix))
            {
                await client.SendTextMessageAsync(chatId, "Такая команда уже занята ботом, извини", ParseMode.Markdown);
                return;
            }

            var content = data[2];

            for (int i = 3; i < data.Length; i++)
            {
                content += " " + data[i];
            }

            if (await RepositoryContainer.Command.CommandExists(prefix, chatId))
            {
                await client.SendTextMessageAsync(chatId, $"Команда `{prefix}` уже существует!", ParseMode.Markdown);
            }
            else
            {
                if (message.From != null && message.From.Username != null)
                    await RepositoryContainer.Command.AddCommand(chatId, message.From.Username, prefix, content);
                else
                    await client.SendTextMessageAsync(chatId, $"Неизвестный пользователь");

                if (!RepositoryContainer.CommandMap.ContainsKey(chatId))
                {
                    RepositoryContainer.CommandMap[chatId] = new List<PrefixContent>();
                }

                RepositoryContainer.CommandMap[chatId].Add(new PrefixContent
                {
                    Prefix = prefix,
                    Content = content
                });

                await client.SendTextMessageAsync(chatId, $"Команда `{prefix}` добавлена успешно", ParseMode.Markdown);
            }
        }
    }
}