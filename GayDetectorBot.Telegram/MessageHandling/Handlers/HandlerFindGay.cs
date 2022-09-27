using GayDetectorBot.Telegram.Data.Repos;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GayDetectorBot.Telegram.MessageHandling.Handlers
{
    [MessageHandler("ктопидор", "узнать пидора дня", MemberStatusPermission.All)]
    public class HandlerFindGay : HandlerBase
    {
        private static List<string[]> _phrases = new List<string[]>
        {
            new[] 
            {
                "Woob-woob, that\'s the sound of the pidor police!",
                "Выезжаю на место...",
                "Но кто же он?"
            },
            new[]
            {
                "Выполняю поиск пидора",
                "Ведётся поиск в базе данных",
                "Ведётся захват подозреваемого..."
            },
            new[]
            {
                "Что тут у нас?",
                "А могли бы на работе делом заниматься...",
                "Проверяю данные.."
            },
            new[]
            {
                "Инициирую поиск пидора дня...",
                "Машины выехали",
                "Так-так, что же тут у нас...",
            },
            new[]
            {
                "Что тут у нас?",
                "Военный спутник запущен, коды доступа внутри...",
                "Не может быть!",
            },
        };

        private static string[] _youGayPhrases = new[]
        {
            "А вот и пидор - ",
            "Вот ты и пидор, ",
            "Ну ты и пидор, ",
            "Сегодня ты пидор, ",
            "Анализ завершен, сегодня ты пидор, ",
            "ВЖУХ И ТЫ ПИДОР, ",
            "Пидор дня обыкновенный, 1шт. - ",
            "Стоять! Не двигаться! Вы объявлены пидором дня, ",
            "И прекрасный человек дня сегодня... а, нет, ошибка, всего лишь пидор - ",
        };

        private Random _random = new Random();

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
                                                              $"Следующее обновление через {delta.Hours}:{delta.Minutes.ToString().PadLeft(2, '0')}");
                    return;
                }
                else
                {
                    throw Error($"Пидор не обнаружен");
                }
            }

            var firstMsg = GetRandomPhrase(0);
            var secondMsg = GetRandomPhrase(1);
            var thirdMsg = GetRandomPhrase(2);

            await SendTextAsync(firstMsg);

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
            await SendTextAsync(secondMsg);
            await Task.Delay(1000);
            await SendTextAsync(thirdMsg);
            await Task.Delay(1000);

            var resI = _random.Next(0, _youGayPhrases.Length);

            await SendTextAsync($"{_youGayPhrases[resI]}@{p.Username}");
        }

        private string GetRandomPhrase(int index)
        {
            var r = _random.Next(0, 3);

            return _phrases[r][index];
        }
    }
}