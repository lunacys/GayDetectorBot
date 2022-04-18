using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram.MessageHandlers
{
    [MessageHandler("команды", "список всех пользовательских команд", MemberStatusPermission.All)]
    public class HandlerCommandList : IMessageHandler
    {
        public string CommandString => "!команды";

        public MemberStatusPermission Permissions =>
            MemberStatusPermission.Administrator | MemberStatusPermission.Creator;

        public bool HasParameters => false;

        private readonly CommandMap _commandMap;

        public HandlerCommandList(CommandMap commandMap)
        {
            _commandMap = commandMap;
        }

        public async Task HandleAsync(Message message, ITelegramBotClient client)
        {
            var chatId = message.Chat.Id;

            var map = _commandMap[chatId];

            var msg = "Все кастомные команды:\n";

            foreach (var pc in map)
            {
                msg += $" - `{pc.Prefix}`\n";
            }

            await client.SendTextMessageAsync(chatId, msg, ParseMode.Markdown);
        }
    }
}