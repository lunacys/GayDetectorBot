using System.Security.Cryptography;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GayDetectorBot.WebApi.Tg.Handlers.DickSize;

[MessageHandler("член")]
[MessageHandlerMetadata("отмерить размер своего полового органа", CommandCategories.DickSize)]
[MessageHandlerPermission(MemberStatusPermission.All)]
public class HandlerMeasureDickSize : HandlerBase
{
    private static readonly string[] _phrases =
    {
        "Чупачупс", "Лоллипап", "Нагибатель", "Пенис", "Стручок", "Член",
        "Морковка", "Экскалибур", "Членохер", "Хер", "Елда", "Талант",
        "Чупачупс", "Хоботок", "Писюлька", "Пиписька", "Пиструн", "Питон в кустах",
        "Шишка", "Шланг ", "Козырь в рукаве", "Удав", "Лысый", "Авторитет",
        "Половой орган", "Мужской половой хуй", "Хуй", "Младший", "Прибор",
        "Ебатель", "Кожаная игла", "Пистон", "Дуло", "Шмайсер", "Хохотунчик",
        "Головастик", "Мундштук", "Красная шапочка", "Питательный тюбик",
        "Хрящ любви", "Щуп", "Долбило", "Одноглазый пиратик", "Ёжик малиновый",
        "Эбонитовая палочка", "Главтрах", "Вставень", "Брызгалка", "Агрессор",
        "Безделушка", "Базука", "Боеголовка", "Любовный мускул", "Старая кубинская сигара",
    };

    private static readonly string[] _sadEmojis =
    [
        "😔", "😞", "🙁", "😒", "😣"
    ];

    private static readonly string[] _happyEmojis =
    [
        "😎", "😏", "😱", "😨", "😯", "😁"
    ];

    private const int MinSize = 1;
    private const int MaxSize = 30;

    private Random _rnd = new Random();

    public override async Task HandleAsync(Message message, params string[] parsedData)
    {
        if (message.From == null)
        {
            throw Error("Неизвестный пользователь");
        }

        var userId = message.From.Id;
        var measurement = GetDailyMeasurement(userId);

        var randomText = _phrases[_rnd.Next(_phrases.Length)];

        var emoji = measurement >= 17 ? _happyEmojis[_rnd.Next(_happyEmojis.Length)] : _sadEmojis[_rnd.Next(_sadEmojis.Length)];

        await SendTextAsync($"{randomText} у тебя **{measurement} см** {emoji}", message.MessageId, ParseMode.MarkdownV2);
    }

    private int GetDailyMeasurement(long userId)
    {
        var today = DateTime.UtcNow.Date;
        string seedString = $"{today:yyyy-MM-dd}-{userId}";

        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(seedString));
        uint hashValue = BitConverter.ToUInt32(hashBytes, 0);
        
        return (int)(hashValue % MaxSize) + MinSize;
    }
}