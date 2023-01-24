namespace GayDetectorBot.WebApi.Configuration;

public class TelegramOptions
{
    public string Token { get; set; } = null!;
    public string PhotoPath { get; set; } = null!;
    public string VideoPath { get; set; } = null!; 
    public string DocumentPath { get; set; } = null!;
    public string HostAddress { get; set; } = null!;
    public string Route { get; set; } = null!;
    public string SecretToken { get; set; } = null!;
    public bool UseWebhooks { get; set; }
    public string AudioPath { get; set; } = null!;
    public string VoicePath { get; set; } = null!;
}