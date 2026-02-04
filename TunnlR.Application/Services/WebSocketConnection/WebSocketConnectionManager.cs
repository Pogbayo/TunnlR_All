using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using TunnlR.Application.Interfaces.IService;

namespace TunnlR.Application.Services.WebSocketConnection
{
    public class WebSocketConnectionManager : IWebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocket> _connections = new();

        public void AddConnection(Guid tunnelId, WebSocket socket)
        {
            _connections.TryAdd(tunnelId.ToString(), socket);
        }

        public WebSocket? GetConnection(Guid tunnelId)
        {
            _connections.TryGetValue(tunnelId.ToString(), out var socket);
            return socket;
        }

        public void RemoveConnection(Guid tunnelId)
        {
            _connections.TryRemove(tunnelId.ToString(), out _);
        }

        public async Task SendMessageAsync(Guid tunnelId, string message)
        {
            var socket = GetConnection(tunnelId);
            if (socket?.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }

        // Optional: Broadcast to all active tunnels
        public async Task BroadcastAsync(string message)
        {
            var tasks = _connections.Values
                .Where(s => s.State == WebSocketState.Open)
                .Select(async socket =>
                {
                    var bytes = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                });
            await Task.WhenAll(tasks);
        }
    }
}