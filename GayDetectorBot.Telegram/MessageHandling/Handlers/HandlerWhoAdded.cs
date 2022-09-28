using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers;

[MessageHandler("кто-добавил", "узнать, кто добавил кастомную команду", MemberStatusPermission.All, "название-команды")]
public class HandlerWhoAdded : HandlerBase<string>
{
    public HandlerWhoAdded(RepositoryContainer repositoryContainer)
        : base(repositoryContainer)
    {
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

        if (RepositoryContainer.ReservedCommands.Contains(prefix))
        {
            await SendTextAsync("Эта команда вшита в бота", message.MessageId);
            return;
        }

        if (!(await RepositoryContainer.Command.CommandExists(prefix, chatId)))
        {
            throw Error($"Команды `{prefix}` не существует!");
        }

        var cmds = await RepositoryContainer.Command.RetrieveAllCommandsByChatId(chatId);
        var cmd = cmds.First(command => command.CommandPrefix == prefix);

        await SendTextAsync($"Команду добавил @{cmd.UserAddedName}", message.MessageId);
    }
}