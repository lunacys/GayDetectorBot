using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GayDetectorBot.WebApi.Models.Tg;

public class TgUser
{
    [Key]
    public long UserId { get; set; }
    
    public string? Username { get; set; }

    public long TotalMessages { get; set; }
    
    public long ContentSent { get; set; }

    [DataType(DataType.DateTime)]
    public DateTime? LastActivity { get; set; }
    
    public long? LastActivityMessageId { get; set; }
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
}