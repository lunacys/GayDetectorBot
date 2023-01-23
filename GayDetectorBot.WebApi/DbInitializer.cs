using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Data;
using GayDetectorBot.WebApi.Models.Users;

namespace GayDetectorBot.WebApi;

public static class DbInitializer
{
    public static void Initialize(GayDetectorBotContext context, AppSettings settings)
    {
        context.Database.EnsureCreated();

        if (!context.Users.Any())
        {
            var users = new[]
            {
                new User
                {
                    Username = settings.DefaultUserName,
                    Email = settings.DefaultUserEmail,
                    PasswordHash = settings.DefaultUserPasswordHash
                }
            };

            foreach (var user in users)
            {
                context.Users.Add(user);
            }

            context.SaveChanges();
        }
    }
}