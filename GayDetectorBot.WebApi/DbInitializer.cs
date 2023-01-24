using GayDetectorBot.WebApi.Configuration;
using GayDetectorBot.WebApi.Data;
using GayDetectorBot.WebApi.Models.Tg;
using GayDetectorBot.WebApi.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace GayDetectorBot.WebApi;

public static class DbInitializer
{
    public static async Task Initialize(GayDetectorBotContext context, AppSettings settings)
    {
        await context.Database.EnsureCreatedAsync();

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

            await context.SaveChangesAsync();
        }

        await AddOldData(context);
    }

    private static async Task AddOldData(GayDetectorBotContext context)
    {
        await AddChats(context);
        await AddCommands(context);
        await AddParticipants(context);
        await AddGays(context);
    }

    private static async Task AddChats(GayDetectorBotContext context)
    {
        if (!File.Exists("SeedData/DB_Chats.txt"))
            return;

        if (await context.Chats.AnyAsync())
            return;

        var content = await File.ReadAllTextAsync("SeedData/DB_Chats.txt");

        var data = content.Split('}');

        for (var i = 0; i < data.Length; i++)
        {
            var s = data[i];
            if (string.IsNullOrEmpty(s) || s == "\n")
                continue;

            var s2 = i == 0 ? s.Remove(0, 1) : s.Remove(0, 2);
            var split = s2.Split('|');

            var chat = new Chat
            {
                Id = int.Parse(split[0]),
                ChatId = long.Parse(split[1]),
                LastGayUsername = split[2],
                LastChecked = DateTimeOffset.Parse(split[3])
            };

            await context.Chats.AddAsync(chat);
        }

        await context.SaveChangesAsync();
    }

    private static async Task AddCommands(GayDetectorBotContext context)
    {
        if (!File.Exists("SeedData/DB_Commands.txt"))
            return;

        if (await context.CustomCommands.AnyAsync())
            return;

        var content = await File.ReadAllTextAsync("SeedData/DB_Commands.txt");

        var data = content.Split('}');

        for (var i = 0; i < data.Length; i++)
        {
            var s = data[i];
            if (string.IsNullOrEmpty(s) || s == "\n")
                continue;
            var s2 = i == 0 ? s.Remove(0, 1) : s.Remove(0, 2);
            var split = s2.Split('|');

            var cmd = new CustomCommand
            {
                Id = int.Parse(split[0]),
                ChatId = long.Parse(split[1]),
                UserAddedName = split[2],
                CommandPrefix = split[3],
                CommandContent = split[4]
            };

            await context.CustomCommands.AddAsync(cmd);
        }

        await context.SaveChangesAsync();
    }

    private static async Task AddParticipants(GayDetectorBotContext context)
    {
        if (!File.Exists("SeedData/DB_Participants.txt"))
            return;

        if (await context.Participants.AnyAsync())
            return;

        var content = await File.ReadAllTextAsync("SeedData/DB_Participants.txt");

        var data = content.Split('}');

        for (var i = 0; i < data.Length; i++)
        {
            var s = data[i];
            if (string.IsNullOrEmpty(s) || s == "\n")
                continue;
            var s2 = i == 0 ? s.Remove(0, 1) : s.Remove(0, 2);

            var split = s2.Split('|');

            var part = new Participant
            {
                Id = int.Parse(split[0]),
                ChatId = long.Parse(split[1]),
                Username = split[2],
                StartedAt = DateTimeOffset.Parse(split[3]),
                IsRemoved = bool.Parse(split[4]),
                FirstName = split[5] == "NULL" ? null : split[5],
                LastName = split[6] == "NULL" ? null : split[6],
            };

            await context.Participants.AddRangeAsync(part);
        }

        await context.SaveChangesAsync();
    }

    private static async Task AddGays(GayDetectorBotContext context)
    {
        if (!File.Exists("SeedData/DB_Gays.txt"))
            return;

        if (await context.Gays.AnyAsync())
            return;

        var content = await File.ReadAllTextAsync("SeedData/DB_Gays.txt");

        var data = content.Split('}');

        for (var i = 0; i < data.Length; i++)
        {
            var s = data[i];
            if (string.IsNullOrEmpty(s) || s == "\n")
                continue;
            var s2 = i == 0 ? s.Remove(0, 1) : s.Remove(0, 2);
            var split = s2.Split('|');

            var p = context.Participants.First(a => a.Id == int.Parse(split[2]));

            var gay = new Gay
            {
                Id = int.Parse(split[0]),
                DateTimestamp = DateTimeOffset.Parse(split[1]),
                Participant = p,
                ParticipantId = p.Id
            };

            await context.Gays.AddAsync(gay);
        }

        await context.SaveChangesAsync();
    }
}