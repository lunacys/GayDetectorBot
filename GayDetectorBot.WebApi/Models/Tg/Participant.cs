using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GayDetectorBot.WebApi.Models.Tg;

public class Participant
{
    [Key]
    public int Id { get; set; } 

    [Required]
    public long ChatId { get; set; }

    [Required]
    public string Username { get; set; } = null!;

    [Required]
    public DateTimeOffset StartedAt { get; set; }

    public bool IsRemoved { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public ICollection<Gay> Gays { get; set; } = null!;
}