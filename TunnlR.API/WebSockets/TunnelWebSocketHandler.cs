using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TunnlR.Application.DTOs.Tunnel;
using TunnlR.Application.Interfaces.IService;


namespace TunnlR.API.WebSockets
{
    public class TunnelWebSocketHandler
    {
        private readonly IWebSocketConnectionManager _connectionManager;
        private readonly ITunnelService _tunnelService;

        public TunnelWebSocketHandler(
            IWebSocketConnectionManager connectionManager,
            ITunnelService tunnelService)
        {
            _connectionManager = connectionManager;
            _tunnelService = tunnelService;
        }

        public async Task HandleConnectionAsync(
            HttpContext context,
            System.Net.WebSockets.WebSocket webSocket)
        {
            var connectionId = Guid.NewGuid().ToString();
            _connectionManager.AddConnection(connectionId, webSocket);

            try
            {
                // Extract user ID from token
                var token = context.Request.Query["token"].ToString();
                var userId = ExtractUserIdFromToken(token);

                // Extract port from query
                var port = int.Parse(context.Request.Query["port"].ToString());
                var protocol = context.Request.Query["protocol"].ToString();

                // Create tunnel
                var tunnelResponse = await _tunnelService.CreateTunnelAsync(userId, new TunnelCreateRequest
                {
                    LocalPort = port,
                    Protocol = protocol
                });

                // Send tunnel info to CLI
                var message = JsonSerializer.Serialize(tunnelResponse);
                await _connectionManager.SendMessageAsync(connectionId, message);

                // Keep connection alive and handle messages
                await ReceiveMessagesAsync(webSocket, connectionId);
            }
            finally
            {
                _connectionManager.RemoveConnection(connectionId);
            }
        }

        private async Task ReceiveMessagesAsync(System.Net.WebSockets.WebSocket webSocket, string connectionId)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Handle incoming traffic from CLI
                    // Forward to appropriate destination
                    Console.WriteLine($"Received from {connectionId}: {message}");
                }
            }
        }

        private Guid ExtractUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.First(c => c.Type == "nameid").Value;
            return Guid.Parse(userIdClaim);
        }
    }
}