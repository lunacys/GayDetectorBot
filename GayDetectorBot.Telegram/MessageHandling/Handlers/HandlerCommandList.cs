﻿using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("команды", "список всех пользовательских команд", MemberStatusPermission.All)]
    public class HandlerCommandList : HandlerBase
    {
        public HandlerCommandList(RepositoryContainer repositoryContainer)
            : base(repositoryContainer) { }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;

            var map = RepositoryContainer.CommandMap[chatId];

            if (map.Count == 0)
            {
                await SendTextAsync("Список команд пуст", message.MessageId);
                return;
            }

            var msg = "Все кастомные команды:\n";

            foreach (var pc in map)
            {
                msg += $" - `{pc.Prefix}`\n";
            }

            await SendTextAsync(msg, message.MessageId);
        }
    }
}