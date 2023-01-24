using System.ComponentModel.DataAnnotations;

namespace GayDetectorBot.WebApi.Models.Tg;

public enum SavedFileType
{
    Photo, Audio, Video, Document, Voice
}

public class SavedFile
{
    [Key]
    public int Id { get; set; }
    [Required] 
    public string FileId { get; set; } = null!;
    [Required]
    public SavedFileType Type { get; set; }
}