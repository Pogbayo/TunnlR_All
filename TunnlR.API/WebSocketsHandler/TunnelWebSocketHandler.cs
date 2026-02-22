using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Contract.DTOs.Auth;
using TunnlR.Contract.DTOs.TunnelDto;

namespace TunnlR.API.WebSockets
{
    public class TunnelWebSocketHandler
    {
        private readonly IWebSocketConnectionManager _connectionManager;
        private readonly ITunnelService _tunnelService;
        private readonly IConfiguration _configuration;
        private static readonly ConcurrentDictionary<Guid, TaskCompletionSource<string>> _pendingRequests = new();

        public TunnelWebSocketHandler(
             IConfiguration configuration,
            IWebSocketConnectionManager connectionManager,
            ITunnelService tunnelService)
        {
            _connectionManager = connectionManager;
            _tunnelService = tunnelService;
            _configuration = configuration;
        }

        public async Task HandleConnectionAsync(HttpContext context,WebSocket webSocket)
        {
            var tunnelId = Guid.Empty;
            var userId = Guid.Empty;
            try
            {
                var token = context.Request.Query["token"].ToString();

                try
                {
                     userId = ExtractUserIdFromToken(token, _configuration);
                }
                catch (UnauthorizedAccessException ex)
                {
                    var errorMsg = new { Type = "ERROR", Reason = ex.Message };
                    var json = JsonSerializer.Serialize(errorMsg);

                    if (webSocket.State == WebSocketState.Open)
                    {
                        var bytes = Encoding.UTF8.GetBytes(json);
                        await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Token expired", CancellationToken.None);
                    }
                    return;
                }

                //will be removed when I implement the UI, but for now I need it to test the tunnel creation
                var port = int.Parse(context.Request.Query["port"].ToString());
                var protocol = context.Request.Query["protocol"].ToString();

                var tunnelResponse = await _tunnelService.CreateTunnelAsync(userId, new TunnelCreateRequest
                {
                    LocalPort = port,
                    Protocol = protocol
                });

                if (tunnelResponse == null || tunnelResponse.TunnelId == Guid.Empty)
                {
                    Console.WriteLine("ERROR: CreateTunnelAsync returned null or empty TunnelId");
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Tunnel creation failed", CancellationToken.None);
                    return;
                }

                tunnelId = tunnelResponse.TunnelId;

                //Console.WriteLine("Relay: WebSocket accepted from CLI. Token: " + token);
                _connectionManager.AddConnection(tunnelId, webSocket);
                Console.WriteLine($"Registered WebSocket for TunnelId {tunnelId}");

                var test = _connectionManager.GetConnection(tunnelId);
                Console.WriteLine($"IMMEDIATE RETRIEVAL: {(test == null ? "NULL" : "FOUND")}");

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
            try
            {
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
                        //Console.WriteLine($"Received from {tunnelId}: {message}");
                    }
                }
            }
            catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely) 
            {
                Console.WriteLine($"Tunnel {tunnelId} disconnected");
            }
            catch (Exception ex) 
            {
                Console.WriteLine($"Error in ReceiveMessages for {tunnelId}: {ex.Message}");
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

        private Guid ExtractUserIdFromToken(string token, IConfiguration configuration)
        {
            var handler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true, 
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
                ClockSkew = TimeSpan.Zero 
            };

            try
            {
                var principal = handler.ValidateToken(token, validationParameters, out _);
                var userIdClaim = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    throw new InvalidOperationException("JWT token does not contain 'NameIdentifier' claim.");

                return Guid.Parse(userIdClaim);
            }
            catch (SecurityTokenExpiredException)
            {
                throw new UnauthorizedAccessException("Token has expired. Please login again.");
            }
            catch (SecurityTokenException)
            {
                throw new UnauthorizedAccessException("Invalid token.");
            }
        }
    }
}