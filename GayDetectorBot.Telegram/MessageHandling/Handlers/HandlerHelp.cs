using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("помоги", "увидеть это сообщение ещё раз", MemberStatusPermission.All)]
    public class HandlerHelp : HandlerBase
    {
        public HandlerHelp(RepositoryContainer repositoryContainer)
            : base(repositoryContainer)
        { }

        public override async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            await client.SendTextMessageAsync(message.Chat.Id, "Тебе уже ничто не поможет...\n\n" +
                                                   "`!добавить <mention>` - добавить пользователя в список рулетки с ссылкой на него\n" +
                                                   "`!ктопидор` - узнать пидора дня\n" +
                                                   "`!топпидоров` - узнать топ пидоров за всё время\n" +
                                                   "`!помоги` - увидеть это сообщение ещё раз\n" +
                                                   "`!уберименя` - убрать из списка рулетки - команда только для настоящих пидоров\n" +
                                                   "`!добавить-команду !<название-команды> <текстовое содержание>` - добавить кастомную команду\n" +
                                                   "`!удалить-команду !<название-команды>` - удалить кастомную команду\n" +
                                                   "`!рандом` - выполнить случайную команду из списка всех команд\n" +
                                                   "`!команды` - список всех пользовательских команд\n" +
                                                   "`!участники` - список всех участников", ParseMode.Markdown);
        }
    }
}