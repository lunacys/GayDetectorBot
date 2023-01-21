namespace GayDetectorBot.Telegram.Services;

public class SchedulerContext
{
    public string Message { get; set; } = null!;
    public int MessageId { get; set; }
    public long ChatId { get; set; }
}