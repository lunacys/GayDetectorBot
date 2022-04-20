using GayDetectorBot.Telegram.Data.Repos;

namespace GayDetectorBot.Telegram.MessageHandling;

public class RepositoryContainer
{
    public ChatRepository Chat { get; }
    public CommandRepository Command { get; } 
    public GayRepository Gay { get; } 
    public ParticipantRepository Participant { get; }

    public CommandMap CommandMap { get; }

    public IEnumerable<string> ReservedCommands { get; }

    public RepositoryContainer(
        ChatRepository chatRepository, 
        CommandRepository commandRepository,
        GayRepository gayRepository,
        ParticipantRepository participantRepository,
        CommandMap commandMap,
        IEnumerable<string> reservedCommands)
    {
        Chat = chatRepository;
        Command = commandRepository;
        Gay = gayRepository;
        Participant = participantRepository;

        CommandMap = commandMap;

        ReservedCommands = reservedCommands;
    }
}