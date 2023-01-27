using GayDetectorBot.WebApi.Data.Repositories;
using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg.Handlers.GayHandling
{
    [MessageHandler("ктопидор")]
    [MessageHandlerMetadata("узнать пидора дня", CommandCategories.Gays)]
    [MessageHandlerPermission(MemberStatusPermission.All)]
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

        private readonly IChatRepository _chatRepository;
        private readonly IParticipantRepository _participantRepository;
        private readonly IGayRepository _gayRepository;

        public HandlerFindGay(IChatRepository chatRepository, IParticipantRepository participantRepository,
            IGayRepository gayRepository)
        {
            _chatRepository = chatRepository;
            _participantRepository = participantRepository;
            _gayRepository = gayRepository;
        }

        public override async Task HandleAsync(Message message, params string[] parsedData)
        {
            var chatId = message.Chat.Id;

            var lastCheck = await _chatRepository.ChatLastChecked(chatId);

            var today = DateTimeOffset.Now;

            if (lastCheck.HasValue && lastCheck.Value.Day == today.Day && lastCheck.Value.Month == today.Month &&
                lastCheck.Value.Year == today.Year)
            {
                var gayToday = await _chatRepository.GetLastGay(chatId);

                if (!string.IsNullOrEmpty(gayToday))
                {
                    var nextDate = today.Date.AddDays(1);

                    var delta = nextDate - today;

                    await SendTextAsync($"Сегодня пидор @{gayToday}\n" +
                                                              $"Следующее обновление через {delta.Hours.ToString().PadLeft(2, '0')}:{delta.Minutes.ToString().PadLeft(2, '0')}", message.MessageId);
                    return;
                }
                else
                {
                    throw Error($"Пидор не обнаружен");
                }
            }

            var firstMsg = GetRandomPhrase(_random.Next(0, _phrases.Count));
            var secondMsg = GetRandomPhrase(_random.Next(0, _phrases.Count));
            var thirdMsg = GetRandomPhrase(_random.Next(0, _phrases.Count));

            await SendTextAsync(firstMsg, null);

            var pList = (await _participantRepository.RetrieveParticipants(chatId)).Where(p => !p.IsRemoved).ToList();

            if (pList.Count == 0)
            {
                throw Error("Участников рулетки нет...");
            }

            var rand = new Random();
            var i = rand.Next(pList.Count);

            var p = pList[i];

            if (!lastCheck.HasValue)
                await _chatRepository.ChatAdd(chatId, DateTimeOffset.Now, p.Username);
            else
                await _chatRepository.ChatUpdate(chatId, p.Username);

            await _gayRepository.AddGay(p);

            await Task.Delay(1000);
            await SendTextAsync(secondMsg, null);
            await Task.Delay(1000);
            await SendTextAsync(thirdMsg, null);
            await Task.Delay(1000);

            var resI = _random.Next(0, _youGayPhrases.Length);

            await SendTextAsync($"{_youGayPhrases[resI]}@{p.Username}", null);
        }

        private string GetRandomPhrase(int index)
        {
            var r = _random.Next(0, 3);

            return _phrases[r][index];
        }
    }
}