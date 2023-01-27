using Telegram.Bot.Types;

namespace GayDetectorBot.WebApi.Tg.Handlers.DickSize;

[MessageHandler("!член")]
[MessageHandlerMetadata("отмерить размер своего полового органа")]
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

    private const int MinSize = 1;
    private const int MaxSize = 50;

    public override async Task HandleAsync(Message message, params string[] parsedData)
    {
        throw new NotImplementedException();
    }
}