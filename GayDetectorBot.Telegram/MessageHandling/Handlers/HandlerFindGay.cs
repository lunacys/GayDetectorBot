using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("ктопидор", "узнать пидора дня", MemberStatusPermission.All)]
    public class HandlerFindGay : HandlerBase
    {
        public HandlerFindGay(RepositoryContainer repositoryContainer)
            : base(repositoryContainer)
        { }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;

            var lastCheck = await RepositoryContainer.Chat.ChatLastChecked(chatId);

            var today = DateTimeOffset.Now;

            if (lastCheck.HasValue && lastCheck.Value.Day == today.Day && lastCheck.Value.Month == today.Month &&
                lastCheck.Value.Year == today.Year)
            {
                var gayToday = await RepositoryContainer.Chat.GetLastGay(chatId);

                if (!string.IsNullOrEmpty(gayToday))
                {
                    var nextDate = today.Date.AddDays(1);

                    var delta = nextDate - today;

                    await SendTextAsync($"Сегодня пидор @{gayToday}\n" +
                                                              $"Следующее обновление через {delta.Hours}:{delta.Minutes}");
                    return;
                }
                else
                {
                    throw Error($"Пидор не обнаружен");
                }
            }

            await SendTextAsync("Выполняю поиск пидора...");

            var pList = (await RepositoryContainer.Participant.RetrieveParticipants(chatId)).Where(p => !p.IsRemoved).ToList();

            if (pList.Count == 0)
            {
                throw Error("Участников рулетки нет...");
            }

            var rand = new Random();
            var i = rand.Next(pList.Count);

            var p = pList[i];

            if (!lastCheck.HasValue)
                await RepositoryContainer.Chat.ChatAdd(chatId, DateTimeOffset.Now, p.Username);
            else
                await RepositoryContainer.Chat.ChatUpdate(chatId, p.Username);

            await RepositoryContainer.Gay.AddGay(p);

            await Task.Delay(1000);
            await SendTextAsync("Пидор обнаружен");
            await Task.Delay(1000);

            await SendTextAsync($"Сегодня ты пидор, @{p.Username}");
        }
    }
}