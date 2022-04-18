namespace GayDetectorBot.Telegram.Models
{
    public class CustomCommand
    {
        public int CommandId { get; set; }
        public long ChatId { get; set; }
        public string UserAddedName { get; set; }
        public string CommandPrefix { get; set; }
        public string CommandContent { get; set; }
    }
}