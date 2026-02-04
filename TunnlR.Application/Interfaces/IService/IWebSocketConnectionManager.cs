

using System.Net.WebSockets;

namespace TunnlR.Application.Interfaces.IService
{
    public interface IWebSocketConnectionManager
    {
        void AddConnection(Guid TunnelId, WebSocket socket);
        WebSocket? GetConnection(Guid TunnelId);
        void RemoveConnection(Guid TunnelId);
        Task SendMessageAsync(Guid TunnelId, string message);
        Task BroadcastAsync(string message);
    }
}
