using Esprima;
using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.WebApi.Services.Tg;

public class TelegramService : ITelegramService
{
    private readonly ILogger<TelegramService> _logger;
    private TelegramBotClient _telegramClient = null!;
    private readonly TelegramOptions _tgOptions;
    private readonly IMessageHandlerService _messageHandler;

    public ITelegramBotClient Client => _telegramClient;

    public TelegramService(ILogger<TelegramService> logger, IOptions<TelegramOptions> tgOptions, IMessageHandlerService messageHandler)
    {
        _logger = logger;
        _tgOptions = tgOptions.Value;
        _messageHandler = messageHandler;
    }

    public async Task Initialize()
    {
        _telegramClient = new TelegramBotClient(_tgOptions.Token.Trim());

        if (_tgOptions.UseWebhooks)
            return;

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };

        _telegramClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);

        var me = await _telegramClient.GetMeAsync();
        _logger.LogInformation($"BOT DATA: {me.Id} {me.FirstName} {me.LastName}");
    }

    public Task HandleUpdateFromController(Update update, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    private Task ErrorHandler(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        var errorMessage = exception switch
        {
            ApiRequestException api => $"Telegram API Error:\n{api.ErrorCode}\n{api.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);

        return Task.CompletedTask;
    }

    private async Task UpdateHandler(ITelegramBotClient client, Update update, CancellationToken token)
    {
        if (update.Type != UpdateType.Message)
            return;

        //if (update.Message!.Type != MessageType.Text)
        //    return;

        if (update.Message == null)
            return;

        var chatId = update.Message.Chat.Id;
        var text = update.Message.Text;
        var from = update.Message.From;

        _logger.LogInformation($"Received a '{text}' message in chat {chatId} from {from?.FirstName} {from?.LastName} @{from?.Username}");

        await _messageHandler.Message(update.Message, client);
    }
}