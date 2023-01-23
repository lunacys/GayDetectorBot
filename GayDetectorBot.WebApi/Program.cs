
using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Data;
using GayDetectorBot.WebApi.Services.Tg;
using GayDetectorBot.WebApi.Services.Tg.Helpers;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Microsoft.Extensions.Options;

namespace GayDetectorBot.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            await InitializeAsync(host);

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());

        private static async Task InitializeAsync(IHost host)
        {
            await using var scope = host.Services.CreateAsyncScope();

            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<GayDetectorBotContext>();
                await context.Database.EnsureCreatedAsync();

                var settings = services.GetRequiredService<IOptions<AppSettings>>();
                DbInitializer.Initialize(context, settings.Value);

                var commandMapService = services.GetRequiredService<ICommandMapService>();
                await commandMapService.Initialize();

                var schedulerService = services.GetRequiredService<ISchedulerService>();
                await schedulerService.Initialize();

                var telegramService = services.GetRequiredService<ITelegramService>();
                await telegramService.Initialize();
            }
            catch (Exception e)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(e, "An error occurred while creating the DB");
            }
        }
    }
}