using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GayDetectorBot.WebApi.Models.Tg;

public class CustomCommand
{
    [Key]
    public int Id { get; set; }
    [Required]
    public long ChatId { get; set; }
    [Required]
    public string UserAddedName { get; set; } = null!;
    [Required]
    public string CommandPrefix { get; set; } = null!;
    [Required]
    public string CommandContent { get; set; } = null!;
}