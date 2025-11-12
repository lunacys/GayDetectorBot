
using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Data;
using GayDetectorBot.WebApi.Services.Tg;
using GayDetectorBot.WebApi.Services.Tg.Helpers;
using GayDetectorBot.WebApi.Services.Tg.MessageHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GayDetectorBot.WebApi
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            await ApplyMigrations(host);
            await InitializeAsync(host);

            await host.RunAsync();
        }

        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args).ConfigureWebHostDefaults(builder => builder.UseStartup<Startup>());

        private static async Task ApplyMigrations(IHost host)
        {
            using var scope = host.Services.CreateScope();
            var services = scope.ServiceProvider;
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var context = services.GetRequiredService<GayDetectorBotContext>();
                
                var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();
                if (pending.Any())
                {
                    logger.LogInformation("Applying {Count} pending migrations: {Migrations}", pending.Count, string.Join(", ", pending));
                    await context.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully");
                }
                else
                {
                    logger.LogInformation("No pending migrations");
                }
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while applying migrations");
                throw;
            }
        }

        private static async Task InitializeAsync(IHost host)
        {
            await using var scope = host.Services.CreateAsyncScope();
            var services = scope.ServiceProvider;
            
            var logger = services.GetRequiredService<ILogger<Program>>();

            try
            {
                var context = services.GetRequiredService<GayDetectorBotContext>();

                var settings = services.GetRequiredService<IOptions<AppSettings>>();
                await DbInitializer.Initialize(context, settings.Value);
                logger.LogInformation("DB initialized with seed data");

                var commandMapService = services.GetRequiredService<ICommandMapService>();
                await commandMapService.Initialize();
                logger.LogInformation("Command map service initialized");

                var schedulerService = services.GetRequiredService<ISchedulerService>();
                await schedulerService.Initialize();
                logger.LogInformation("Scheduler service initialized");

                var savedFileContainer = services.GetRequiredService<ISavedFileContainer>();
                await savedFileContainer.Initialize();
                logger.LogInformation("File container initialized");

                // var telegramService = services.GetRequiredService<ITelegramService>();
                // await telegramService.Initialize();
                
                var tgUserCache = services.GetRequiredService<ITgUserCache>();
                await tgUserCache.InitializeFromDbAsync();
                logger.LogInformation("TgUser Cache initialized");
                
                logger.LogInformation("All services initialized successfully");
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while initializing services");
                throw;
            }
        }
    }
}