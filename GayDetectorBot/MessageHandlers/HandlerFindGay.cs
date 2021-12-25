using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using GayDetectorBot.Data.Repos;

namespace GayDetectorBot.MessageHandlers
{
    public class HandlerFindGay : IMessageHandler
    {
        public string CommandString => "!ктопидор";

        public bool HasParameters => false;

        private readonly GuildRepository _guildRepository;
        private readonly ParticipantRepository _participantRepository;
        private readonly GayRepository _gayRepository;

        public HandlerFindGay(GuildRepository guildRepository, ParticipantRepository participantRepository, GayRepository gayRepository)
        {
            _guildRepository = guildRepository;
            _participantRepository = participantRepository;
            _gayRepository = gayRepository;
        }

        public async Task HandleAsync(SocketMessage message)
        {
            var ch = message.Channel as SocketGuildChannel;
            var g = ch?.Guild;

            var lastCheck = await _guildRepository.GuildLastChecked(g.Id);

            var today = DateTimeOffset.Now;

            if (lastCheck.HasValue && lastCheck.Value.Day == today.Day && lastCheck.Value.Month == today.Month &&
                lastCheck.Value.Year == today.Year)
            {
                var gayToday = await _guildRepository.GetLastGay(g.Id);

                if (gayToday.HasValue)
                {
                    var gayUser = await message.Channel.GetUserAsync(gayToday.Value);

                    var nextDate = today.Date.AddDays(1);

                    await message.Channel.SendMessageAsync($"Сегодня пидор {gayUser.Mention}\n" +
                                                           $"Следующее обновление через {(nextDate - today)}");
                    return;
                }
                else
                {
                    await message.Channel.SendMessageAsync($"Пидор не обнаружен");
                }
            }

            await message.Channel.SendMessageAsync("Выполняю поиск пидора...");

            var pList = (await _participantRepository.RetrieveParticipants(g.Id)).Where(p => !p.IsRemoved).ToList();

            var rand = new Random();
            var i = rand.Next(pList.Count);

            var p = pList[i];

            if (!lastCheck.HasValue)
                await _guildRepository.GuildAdd(g.Id, DateTimeOffset.Now, p.UserId);
            else
                await _guildRepository.GuildUpdate(g.Id, p.UserId);

            await _gayRepository.AddGay(p);

            await Task.Delay(1000);
            await message.Channel.SendMessageAsync("Пидор обнаружен");
            await Task.Delay(1000);

            var user = await message.Channel.GetUserAsync(p.UserId);

            await message.Channel.SendMessageAsync($"Сегодня ты пидор, {user.Mention}");
        }
    }
}