using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using TunnlR.Application.Interfaces.IService;

namespace TunnlR.Application.Services.WebSocket
{
    public class WebSocketConnectionManager : IWebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _connections = new();

        public void AddConnection(string connectionId, System.Net.WebSockets.WebSocket socket)
        {
            _connections.TryAdd(connectionId, socket);
        }

        public System.Net.WebSockets.WebSocket? GetConnection(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var socket);
            return socket;
        }

        public void RemoveConnection(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public async Task SendMessageAsync(string connectionId, string message)
        {
            var socket = GetConnection(connectionId);
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