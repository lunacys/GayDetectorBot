using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.WebApi.Tg.Handlers;

[MessageHandler("роли", "получить роли пользователя")]
public class HandlerGetRoles : HandlerBase
{
    public override async Task HandleAsync(Message message, params string[] parsedData)
    {
        var user = message.From;
        if (user == null)
            throw Error("Кто ты???");

        //var chatMember = message.From.Id;

        await SendTextAsync($"Ты у нас: " + user, message.MessageId, ParseMode.Html);
    }
}