using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg.Handlers.CustomCommands
{
    [MessageHandler("удалить-команду", "название-команды")]
    [MessageHandlerMetadata("удалить кастомную команду", CommandCategories.Commands)]
    [MessageHandlerPermission(MemberStatusPermission.All)]
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