using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg.Handlers.CustomCommands;

[MessageHandler("добавить-команду", "название-команды", "текстовое содержание")]
[MessageHandlerMetadata("добавить кастомную команду", CommandCategories.Commands)]
[MessageHandlerPermission(MemberStatusPermission.All)]
public class HandlerAddCommand : HandlerBase<string, string>
{
    private readonly ICommandRepository _commandRepository;
    private readonly ICommandMapService _commandMapService;

    public HandlerAddCommand(ICommandRepository commandRepository, ICommandMapService commandMapService)
    {
        _commandRepository = commandRepository;
        _commandMapService = commandMapService;
    }

    public override async Task HandleAsync(Message message, string? prefix, string? content)
    {
        var chatId = message.Chat.Id;


        if (prefix == null || content == null)
        {
            throw Error("Мало данных! Надо два параметра!");
        }

        if (!prefix.StartsWith('!'))
        {
            throw Error("Команды должны начинаться со знака `!`");
        }

        if (_commandMapService.ReservedCommands.Contains(prefix))
        {
            throw Error("Такая команда уже занята ботом, извини");
        }

        if (await _commandRepository.CommandExists(prefix, chatId))
        {
            throw Error($"Команда `{prefix}` уже существует!");
        }
        else
        {
            if (message.From != null && message.From.Username != null)
                await _commandRepository.AddCommand(chatId, message.From.Username, prefix, content);
            else
                Error($"Неизвестный пользователь");

            if (!_commandMapService.ContainsKey(chatId))
            {
                _commandMapService.SetByChatId(chatId, new List<PrefixContent>());
            }

            _commandMapService.GetByChatId(chatId).Add(new PrefixContent
            {
                Prefix = prefix,
                Content = content
            });

            await SendTextAsync($"Команда `{prefix}` добавлена успешно", null);
        }
    }
}