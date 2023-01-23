using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GayDetectorBot.WebApi.Models.Tg;

public class Gay
{
    [Key]
    public int Id { get; set; }

    [Required]
    [DataType(DataType.DateTime)]
    public DateTimeOffset DateTimestamp { get; set; }

    [Required]
    [ForeignKey("ParticipantId")]
    public Participant Participant { get; set; } = null!;

    public int ParticipantId { get; set; }
}