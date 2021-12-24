namespace GayDetectorBot.Models
{
    public class CustomCommand
    {
        public int CommandId { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserAddedId { get; set; }
        public string CommandPrefix { get; set; }
        public string CommandContent { get; set; }
    }
}