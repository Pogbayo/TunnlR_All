using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Contract.DTOs.Auth;
using TunnlR.Contract.DTOs.TunnelDto;

namespace TunnlR.API.WebSockets
{
    public class TunnelWebSocketHandler
    {
        private readonly IWebSocketConnectionManager _connectionManager;
        private readonly ITunnelService _tunnelService;
        private static readonly ConcurrentDictionary<Guid, TaskCompletionSource<string>> _pendingRequests = new();

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

                    try
                    {
                        var responseData = JsonSerializer.Deserialize<HttpResponseData>(message);

                        if (responseData?.RequestId != null)
                        {
                            if (_pendingRequests.TryRemove(responseData.RequestId, out var tcs))
                            {
                                tcs.SetResult(responseData.Body);
                            }
                        }
                    }
                    catch
                    {
                        Console.WriteLine($"Received from {tunnelId}: {message}");
                    }
                    Console.WriteLine($"Received from {tunnelId}: {message}");
                }
            }
        }

        public static async Task<string> WaitForResponse(Guid requestId, TimeSpan timeout)
        {
            var tcs = new TaskCompletionSource<string>();

            _pendingRequests[requestId] = tcs;

            var delayTask = Task.Delay(timeout);
            var responseTask = tcs.Task;

            var completedTask = await Task.WhenAny(responseTask, delayTask);

            if (completedTask == delayTask)
            {
                _pendingRequests.TryRemove(requestId, out _);
                throw new TimeoutException("CLI did not respond in time");
            }

            return await responseTask;
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