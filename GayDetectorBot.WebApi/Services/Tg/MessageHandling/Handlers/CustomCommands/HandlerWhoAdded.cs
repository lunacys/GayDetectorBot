using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling.Handlers.CustomCommands;

[MessageHandler("кто-добавил", "узнать, кто добавил кастомную команду", MemberStatusPermission.All, "название-команды")]
public class HandlerWhoAdded : HandlerBase<string>
{
    private readonly ICommandMapService _commandMapService;
    private readonly ICommandRepository _commandRepository;

    public HandlerWhoAdded(ICommandMapService commandMapService, ICommandRepository commandRepository)
    {
        _commandMapService = commandMapService;
        _commandRepository = commandRepository;
    }

    public override async Task HandleAsync(Message message, string? prefix)
    {
        var chatId = message.Chat.Id;


        if (prefix == null)
        {
            throw Error("Забыл указать команду");
        }

        if (!prefix.StartsWith('!'))
        {
            throw Error("Команды должны начинаться со знака `!`");
        }

        if (_commandMapService.ReservedCommands.Contains(prefix))
        {
            await SendTextAsync("Эта команда вшита в бота", message.MessageId);
            return;
        }

        if (!await _commandRepository.CommandExists(prefix, chatId))
        {
            throw Error($"Команды `{prefix}` не существует!");
        }

        var cmds = await _commandRepository.RetrieveAllCommandsByChatId(chatId);
        var cmd = cmds.First(command => command.CommandPrefix == prefix);

        await SendTextAsync($"Команду добавил @{cmd.UserAddedName}", message.MessageId);
    }
}