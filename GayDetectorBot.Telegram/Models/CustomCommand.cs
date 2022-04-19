namespace GayDetectorBot.Telegram.Models
{
    public class CustomCommand
    {
        public int CommandId { get; set; }
        public long ChatId { get; set; }
        public string UserAddedName { get; set; } = null!;
        public string CommandPrefix { get; set; } = null!;
        public string CommandContent { get; set; } = null!;
    }
}