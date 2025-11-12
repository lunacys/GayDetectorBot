using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GayDetectorBot.WebApi.Models.Tg;

public enum PointsReason
{
    /// <summary>
    /// Regular chatting.
    /// </summary>
    Chatting = 0,
    /// <summary>
    /// Sent content, e.g. photos, videos, etc.
    /// </summary>
    Content = 1,
    /// <summary>
    /// Gifted points from someone else.
    /// </summary>
    Gift = 2,
    /// <summary>
    /// Gained for playing bot's games.
    /// </summary>
    Gaming = 3
}

public class Points
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public long ChatId { get; set; }

    [Required]
    [ForeignKey(nameof(ChatId))]
    public Chat Chat { get; set; } = null!;
    
    [Required]
    public long UserId { get; set; }

    [Required]
    public string Username { get; set; } = null!;
    
    [Required]
    public long Amount { get; set; }
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime CreatedAt { get; set; }
    
    [Required]
    [DataType(DataType.DateTime)]
    public DateTime UpdatedAt { get; set; }
    
    [Required]
    public PointsReason Reason { get; set; }
    
    /// <summary>
    /// Used for storing data about specific reason. For example UserId gifted, game name, etc.
    /// </summary>
    public string? Metadata { get; set; }
}