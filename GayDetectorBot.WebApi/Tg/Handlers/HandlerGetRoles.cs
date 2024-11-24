using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.WebApi.Tg.Handlers;

[MessageHandler("роли")]
[MessageHandlerMetadata("получить роли пользователя")]
[MessageHandlerPermission(MemberStatusPermission.Regular)]
public class HandlerGetRoles : HandlerBase
{
    public override async Task HandleAsync(Message message, params string[] parsedData)
    {
        var user = message.From;
        if (user == null)
            throw Error("Кто ты???");

        var chatMember = await Client.GetChatMember(ChatId, user.Id);
        await SendTextAsync($"Ты у нас: {user} - {chatMember.Status}", message.MessageId, ParseMode.Html);
    }
}