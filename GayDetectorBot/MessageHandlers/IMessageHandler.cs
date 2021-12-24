using System.Threading.Tasks;
using Discord.WebSocket;

namespace GayDetectorBot.MessageHandlers
{
    public interface IMessageHandler
    {
        Task HandleAsync(SocketMessage message);
    }
}