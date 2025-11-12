using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GayDetectorBot.WebApi.Models.Tg;

public class TgUserChatLink
{
    [Key]
    public int Id { get; set; }

    [ForeignKey(nameof(UserId))]
    public TgUser User { get; set; } = null!;
    public long UserId { get; set; }
    
    [ForeignKey(nameof(ChatId))]
    public Chat Chat { get; set; } = null!;
    public long ChatId { get; set; }
    
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
    
    public bool IsRemoved { get; set; }
}