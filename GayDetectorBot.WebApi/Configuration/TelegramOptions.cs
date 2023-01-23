namespace GayDetectorBot.WebApi.Configuration;

public class TelegramOptions
{
    public string Token { get; set; } = null!;
    public string PhotoPath { get; set; } = null!;
    public string VideoPath { get; set; } = null!; 
    public string DocumentPath { get; set; } = null!;
}