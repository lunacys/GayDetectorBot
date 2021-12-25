using System.Threading.Tasks;
using Discord.WebSocket;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerHelp : IMessageHandler
    {
        public string CommandString => "!помоги";

        public bool HasParameters => false;

        public async Task HandleAsync(SocketMessage message)
        {
            await message.Channel.SendMessageAsync("Тебе уже ничто не поможет...\n\n" +
                                                   "`!добавить <mention>` ИЛИ `!добавить <userid>` - добавить пользователя в список рулетки с ссылкой на него\n" +
                                                   "`!ктопидор` - узнать пидора дня\n" +
                                                   "`!топпидоров` - узнать топ пидоров за всё время\n" +
                                                   "`!помоги` - увидеть это сообщение ещё раз\n" +
                                                   "`!уберименя` - убрать из списка рулетки - команда только для настоящих пидоров\n" +
                                                   "`!добавить-команду !<название-команды> <текстовое содержание>` - добавить кастомную команду\n" +
                                                   "`!удалить-команду !<название-команды>` - удалить кастомную команду\n" +
                                                   "`!рандом` - выполнить случайную команду из списка всех команд\n" +
                                                   "`!команды` - список всех пользовательских команд\n" +
                                                   "`!участники` - список всех участников");
        }
    }
}