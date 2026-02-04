using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TunnlR.Application.DTOs.Tunnel;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Domain.Entities;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        public async Task HandleConnectionAsync(HttpContext context,WebSocket webSocket)
        {
            var tunnelId = Guid.Empty;

            try
            {
                var token = context.Request.Query["token"].ToString();
                var userId = ExtractUserIdFromToken(token);

                var port = int.Parse(context.Request.Query["port"].ToString());
                var protocol = context.Request.Query["protocol"].ToString();

                var tunnelResponse = await _tunnelService.CreateTunnelAsync(userId, new TunnelCreateRequest
                {
                    LocalPort = port,
                    Protocol = protocol
                });
                tunnelId = tunnelResponse.TunnelId;
                _connectionManager.AddConnection(tunnelId, webSocket);

                var message = JsonSerializer.Serialize(tunnelResponse);
                await _connectionManager.SendMessageAsync(tunnelId, message);

                await ReceiveMessagesAsync(webSocket, tunnelId);
            }
            finally
            {
                _connectionManager.RemoveConnection(tunnelId);
            }
        }
         
        private async Task ReceiveMessagesAsync(WebSocket  webSocket, Guid tunnelId)
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
                    Console.WriteLine($"Received from {tunnelId}: {message}");
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