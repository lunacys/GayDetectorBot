﻿using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg.Handlers.CustomCommands
{
    [MessageHandler("команды")]
    [MessageHandlerMetadata("список всех пользовательских команд", CommandCategories.Commands)]
    [MessageHandlerPermission(MemberStatusPermission.All)]
    public class HandlerCommandList : HandlerBase
    {
        private readonly ICommandMapService _commandMapService;

        public HandlerCommandList(ICommandMapService commandMapService)
        {
            _commandMapService = commandMapService;
        }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;

            var map = _commandMapService.GetByChatId(chatId);

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