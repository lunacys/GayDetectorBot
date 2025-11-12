using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Services.Auth;
using GayDetectorBot.WebApi.Services.Tg;
using GayDetectorBot.WebApi.Services.Tg.Helpers;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using GayDetectorBot.WebApi.Services.UserManagement;
using System.Reflection;
using Microsoft.Extensions.Options;

namespace GayDetectorBot.WebApi;

public static class ServiceConfigurator
{
    public static void ConfigureGayDetector(this IServiceCollection services, TelegramOptions telegramOptions)
    {
        services.AddScoped<ITokenGeneratorService, TokenGeneratorService>();
        services.AddScoped<IPasswordHasherService, PasswordHasherService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();

        services.AddSingleton<ITgUserRepository, TgUserRepository>();
        services.AddSingleton<IChatRepository, ChatRepository>();
        services.AddSingleton<ICommandRepository, CommandRepository>();
        services.AddSingleton<IGayRepository, GayRepository>();
        services.AddSingleton<IParticipantRepository, ParticipantRepository>();
        services.AddSingleton<IScheduleRepository, ScheduleRepository>();
        services.AddSingleton<ISavedFileRepository, SavedFileRepository>();
        services.AddSingleton<IPointsRepository, PointsRepository>();
        services.AddSingleton<ITgUserChatLinkRepository, TgUserChatLinkRepository>();
        
        services.AddSingleton<ISavedFileContainer, SavedFileContainer>();

        services.AddSingleton<ISchedulerService, SchedulerService>();
        services.AddSingleton<IJsEvaluatorService, JsEvaluatorService>();

        services.AddSingleton<IHandlerMetadataContainer, HandlerMetadataContainer>();
        services.AddSingleton<ICommandMapService, CommandMapService>();
        
        services.AddSingleton<ITgUserCache, TgUserCache>();
        services.AddSingleton<IPointsStorage, PointsStorage>();

        AddHandlers(services);

        services.AddSingleton<IMessageHandlerService, MessageHandlerService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<MessageHandlerService>>();
            var commandMap = provider.GetRequiredService<ICommandMapService>();
            var hmc = provider.GetRequiredService<IHandlerMetadataContainer>();
            var js = provider.GetRequiredService<IJsEvaluatorService>();
            var sf = provider.GetRequiredService<ISavedFileContainer>();
            var link = provider.GetRequiredService<ITgUserChatLinkRepository>();
            var uc = provider.GetRequiredService<ITgUserCache>();

            return new MessageHandlerService(logger, commandMap, hmc, provider, js, sf, link, uc);
        });
        //services.AddSingleton<ITelegramService, TelegramService>();
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramService>((_, sp) =>
            {
                var logger = sp.GetRequiredService<ILogger<TelegramService>>();
                var tgOpt = sp.GetRequiredService<IOptions<TelegramOptions>>();
                var msgHandler = sp.GetRequiredService<IMessageHandlerService>();

                return new TelegramService(logger, tgOpt, msgHandler);
            });

        services.AddHostedService<WebhookService>();
    }

    private static void AddHandlers(IServiceCollection services)
    {
        var types = HandlerMetadataContainer.GetTypesWithAttribute(Assembly.GetExecutingAssembly());

        foreach (var valueTuple in types)
        {
            services.AddTransient(valueTuple.type);
        }
    }
}