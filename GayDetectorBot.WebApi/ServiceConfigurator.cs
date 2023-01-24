using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Data;
using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Services.Auth;
using GayDetectorBot.WebApi.Services.Tg;
using GayDetectorBot.WebApi.Services.Tg.Helpers;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using GayDetectorBot.WebApi.Services.UserManagement;
using System.Reflection;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling.Handlers;
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

        services.AddSingleton<IChatRepository, ChatRepository>();
        services.AddSingleton<ICommandRepository, CommandRepository>();
        services.AddSingleton<IGayRepository, GayRepository>();
        services.AddSingleton<IParticipantRepository, ParticipantRepository>();
        services.AddSingleton<IScheduleRepository, ScheduleRepository>();

        services.AddSingleton<ISchedulerService, SchedulerService>();
        services.AddSingleton<IJsEvaluatorService, JsEvaluatorService>();

        services.AddSingleton<IHandlerMetadataContainer, HandlerMetadataContainer>();
        services.AddSingleton<ICommandMapService, CommandMapService>();

        AddHandlers(services);

        services.AddSingleton<IMessageHandlerService, MessageHandlerService>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<MessageHandlerService>>();
            var commandMap = provider.GetRequiredService<ICommandMapService>();
            var hmc = provider.GetRequiredService<IHandlerMetadataContainer>();
            var js = provider.GetRequiredService<IJsEvaluatorService>();
            var tgOpt = provider.GetRequiredService<IOptions<TelegramOptions>>();

            return new MessageHandlerService(logger, commandMap, hmc, provider, js, tgOpt);
        });
        //services.AddSingleton<ITelegramService, TelegramService>();
        services.AddHttpClient("telegram_bot_client")
            .AddTypedClient<ITelegramService>((client, sp) =>
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
        services.AddTransient<HandlerAddCommand>();
        services.AddTransient<HandlerAddParticipant>();
        services.AddTransient<HandlerCommandList>();
        services.AddTransient<HandlerDeleteCommand>();
        services.AddTransient<HandlerEval>();
        services.AddTransient<HandlerFindGay>();
        services.AddTransient<HandlerGayOfTheDay>();
        services.AddTransient<HandlerGayTop>();
        services.AddTransient<HandlerGetRoles>();
        services.AddTransient<HandlerParticipants>();
        services.AddTransient<HandlerRandom>();
        services.AddTransient<HandlerRemoveMe>();
        services.AddTransient<HandlerSchedule>();
        services.AddTransient<HandlerWhoAdded>();
    }

    private static IEnumerable<(Type type, MessageHandlerAttribute attribute)> GetTypesWithAttribute(Assembly assembly)
    {
        foreach (var type in assembly.GetTypes())
        {
            var attr = type.GetCustomAttributes(typeof(MessageHandlerAttribute), false);

            if (attr.Length > 0)
                yield return (type, attr[0] as MessageHandlerAttribute)!;
        }
    }
}