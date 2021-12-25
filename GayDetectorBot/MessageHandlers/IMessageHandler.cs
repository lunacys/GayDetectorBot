using System.Threading.Tasks;
using Discord.WebSocket;

namespace GayDetectorBot.MessageHandlers
{
    public interface IMessageHandler
    {
        string CommandString { get; }
        bool HasParameters { get; }
        Task HandleAsync(SocketMessage message);
    }
}