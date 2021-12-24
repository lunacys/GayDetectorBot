using System;

namespace GayDetectorBot.Models
{
    public class Participant
    {
        public int ParticipantId { get; set; }
        public ulong GuildId { get; set; }
        public ulong UserId { get; set; }
        public DateTime StartedAt { get; set; }
        public bool IsRemoved { get; set; }
    }
}