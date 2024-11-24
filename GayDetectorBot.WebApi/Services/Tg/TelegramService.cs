using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Services.Tg;

public interface ITelegramService
{
    ITelegramBotClient Client { get; }

    Task Initialize();

    Task HandleUpdateFromController(Update update, CancellationToken cancellationToken);
}

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

        var me = await _telegramClient.GetMe();
        _logger.LogInformation($"BOT DATA: {me.Id} {me.FirstName} {me.LastName}");

        if (_tgOptions.UseWebhooks)
            return;

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = { }
        };

        _telegramClient.StartReceiving(UpdateHandler, ErrorHandler, receiverOptions);
    }

    public async Task HandleUpdateFromController(Update update, CancellationToken cancellationToken)
    {
        await _messageHandler.Update(update, _telegramClient);
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
        await _messageHandler.Update(update, client);
    }
}