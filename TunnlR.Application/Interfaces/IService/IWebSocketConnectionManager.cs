

namespace TunnlR.Application.Interfaces.IService
{
    public interface IWebSocketConnectionManager
    {
        void AddConnection(string connectionId, System.Net.WebSockets.WebSocket socket);
        System.Net.WebSockets.WebSocket? GetConnection(string connectionId);
        void RemoveConnection(string connectionId);
        Task SendMessageAsync(string connectionId, string message);
        Task BroadcastAsync(string message);
    }
}
