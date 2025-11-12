using System.ComponentModel.DataAnnotations;

namespace GayDetectorBot.WebApi.Models.Tg;

public class Chat
{
    [Key]
    public long ChatId { get; set; }
    public string? LastGayUsername { get; set; }
    public DateTimeOffset? LastChecked { get; set; }
}