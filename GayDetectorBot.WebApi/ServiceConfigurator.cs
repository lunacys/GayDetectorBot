using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Data.Repositories;
using GayDetectorBot.WebApi.Services.Auth;
using GayDetectorBot.WebApi.Services.Tg.Helpers;
using GayDetectorBot.WebApi.Services.UserManagement;

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
    }
}