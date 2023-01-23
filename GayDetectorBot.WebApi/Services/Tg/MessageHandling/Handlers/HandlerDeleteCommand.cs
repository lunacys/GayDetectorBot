using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models.Tg;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling.Handlers
{
    [MessageHandler("удалить-команду", MemberStatusPermission.All, "удалить кастомную команду", "название-команды")]
    public class HandlerDeleteCommand : HandlerBase<string>
    {
        private readonly ICommandMapService _commandMapService;
        private readonly ICommandRepository _commandRepository;

        public HandlerDeleteCommand(ICommandMapService commandMapService, ICommandRepository commandRepository)
        {
            _commandMapService = commandMapService;
            _commandRepository = commandRepository;
        }

        public override async Task HandleAsync(Message message, string? prefix)
        {
            var chatId = message.Chat.Id;

            if (prefix == null)
            {
                throw Error("Мало данных! Нужен один параметр!");
            }

            if (!await _commandRepository.CommandExists(prefix, chatId))
            {
                throw Error($"Команды `{prefix}` не существует");
            }

            if (prefix.Trim().ToLower() == "!вера")
                throw Error("Как ты смеешь???");

            await _commandRepository.DeleteCommand(prefix, chatId);
            _commandMapService.GetByChatId(chatId)?.RemoveAll(pc => pc.Prefix == prefix);

            await SendTextAsync($"Команда `{prefix}` успешно удалена", message.MessageId);
        }
    }
}