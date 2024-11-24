using GayDetectorBot.WebApi.Configuration;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.WebApi.Services.Tg;

public class WebhookService : IHostedService
{
    private readonly ILogger<WebhookService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TelegramOptions _telegramOptions;

    public WebhookService(
        ILogger<WebhookService> logger,
        IServiceProvider serviceProvider,
        IOptions<TelegramOptions> telegramOptions)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _telegramOptions = telegramOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ITelegramService>();

        var webhookAddress = $"{_telegramOptions.HostAddress}{_telegramOptions.Route}";

        _logger.LogInformation($"Setting webhook: {webhookAddress}", webhookAddress);

        await client.Initialize();
        //await client.Client.DeleteWebhookAsync(cancellationToken: cancellationToken);

        if (_telegramOptions.UseWebhooks)
        {
            await client.Client.SetWebhook(
                url: webhookAddress,
                allowedUpdates: Array.Empty<UpdateType>(),
                secretToken: _telegramOptions.SecretToken,
                cancellationToken: cancellationToken
            );
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (!_telegramOptions.UseWebhooks)
            return;

        using var scope = _serviceProvider.CreateScope();
        var client = scope.ServiceProvider.GetRequiredService<ITelegramService>();

        _logger.LogInformation("Removing webhook");
        await client.Client.DeleteWebhook(cancellationToken: cancellationToken);
    }
}