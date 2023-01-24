using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Models.Tg;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using File = System.IO.File;

namespace GayDetectorBot.WebApi.Services.Tg.MessageHandling.Handlers;

[MessageHandler("фото", "скинуть случайную сохранённую фотку", MemberStatusPermission.All)]
public class HandlerRandomPhoto : HandlerBase
{
    private readonly TelegramOptions _options;
    private readonly ISavedFileContainer _savedFileContainer;

    public HandlerRandomPhoto(IOptions<TelegramOptions> options, ISavedFileContainer savedFileContainer)
    {
        _options = options.Value;
        _savedFileContainer = savedFileContainer;
    }

    public override async Task HandleAsync(Message message, params string[] parsedData)
    {
        var chatId = message.Chat.Id;

        var rnd = await _savedFileContainer.GetRandomByType(chatId, SavedFileType.Photo);

        if (rnd == null)
            throw Error("Нету фоток");

        await Client.SendPhotoAsync(chatId, new InputFileId(rnd.FileId), replyToMessageId: message.MessageId);
    }
}