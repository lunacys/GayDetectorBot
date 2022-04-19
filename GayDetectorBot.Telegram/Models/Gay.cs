namespace GayDetectorBot.Telegram.Models
{
    public class Gay
    {
        public int GayId { get; set; }
        public DateTime DateTimestamp { get; set; }
        public Participant Participant { get; set; } = null!;
    }
}