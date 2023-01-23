using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Telegram.Bot;

namespace GayDetectorBot.WebApi;

public static class ServiceProviderExtensions
{
    public static IMessageHandler GetMessageHandler(this IServiceProvider serviceProvider, Type type, long chatId,
        ITelegramBotClient client)
    {
        var service = serviceProvider.GetRequiredService(type) as IMessageHandler;

        if (service == null)
            throw new Exception("Could not find service of type " + type.Name);

        service.ChatId = chatId;
        service.Client = client;

        return service;
    }
}