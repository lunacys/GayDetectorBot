namespace GayDetectorBot.Telegram.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }
        public long ChatId { get; set; }
        public string Username { get; set; } = null!;
        public DateTime StartedAt { get; set; }
        public bool IsRemoved { get; set; }
    }
}