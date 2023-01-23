using System.ComponentModel.DataAnnotations;

namespace GayDetectorBot.WebApi.Models.Tg;

public class SchedulerContext
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string Message { get; set; } = null!;
    [Required]
    public int MessageId { get; set; }
    [Required]
    public long ChatId { get; set; }
    [Required]
    public DateTimeOffset FireTime { get; set; }
}