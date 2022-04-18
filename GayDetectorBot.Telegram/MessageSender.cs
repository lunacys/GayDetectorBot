using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.Telegram;

public class MessageSender
{
    private readonly ITelegramBotClient _client;
    private readonly long _chatId;

    public MessageSender(ITelegramBotClient client, long chatId)
    {
        _client = client;
        _chatId = chatId;
    }

    public async Task SendText(string text, ParseMode parseMode = ParseMode.Markdown)
    {
        await _client.SendTextMessageAsync(_chatId, text, parseMode);
    }
}