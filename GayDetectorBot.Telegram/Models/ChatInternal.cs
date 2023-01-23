namespace GayDetectorBot.Telegram.Models;

public class ChatInternal
{

    public int ChatInternalId { get; set; }
    public long ChatId { get; set; }
    public string? LastGayUsername { get; set; }
    public DateTime? LastChecked { get; set; }
}