using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TunnlR.Contract.DTOs.Auth;
using TunnlR.Contract.DTOs.TunnelDto;

namespace TunnlR.CLI.Services
{
    public class CLITunnelService
    {
        private readonly IConfiguration _configuration;
        private ClientWebSocket? _websocket;
        private int _localPort;

        public event EventHandler<string>? MessageReceived;
        public event EventHandler<TunnelCreateResponse>? TunnelEstablished;
        public event EventHandler? TunnelClosed;
        public event EventHandler? TunnelDeactivated;
        public event EventHandler<string>? TunnelFailed;
        private TaskCompletionSource<bool>? _connectTcs;


        public CLITunnelService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ConnectAsync(string token, int localPort, string protocol)
        {
            _localPort = localPort;
            var wsUrl = _configuration["RelayServer:WebSocketUrl"]!;
            _websocket = new ClientWebSocket();

            //This tells the proxy server...Okay, we strted as HTTP, but from NOW ON, this connection is WebSokcet. Don't close it
            var uri = new Uri($"{wsUrl}?token={token}&port={localPort}&protocol={protocol}");

            _connectTcs = new TaskCompletionSource<bool>();

            try
            {
                await _websocket.ConnectAsync(uri, CancellationToken.None);

                _ = Task.Run(ListenAsync);

                var success = await _connectTcs.Task;

                if (!success)
                {
                    Console.WriteLine("Tunnel connection failed.");
                    return;
                }
            }
            catch (Exception ex)
            {
                TunnelFailed?.Invoke(this, $"Failed to connect to relay server: {ex.Message}");
                _connectTcs.TrySetResult(false);
            }
        }

        private async Task ListenAsync()
        {
            var buffer = new byte[1024 * 4];

            try
            {
                while (_websocket?.State == WebSocketState.Open)
                {
                    var result = await _websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Try parse control message
                    if (TryParseControlMessage(message, out string type, out string? reason))
                    {
                        switch (type)
                        {
                            case "ERROR":
                                TunnelFailed?.Invoke(this, reason ?? "Unknown server error");
                                _connectTcs?.TrySetResult(false);
                                return; // stop ListenAsync
                            case "TUNNEL_CLOSED":
                                TunnelDeactivated?.Invoke(this, EventArgs.Empty);
                                _connectTcs?.TrySetResult(false);
                                return;
                        }
                    }

                    await HandleMessageAsync(message);
                }
            }
            catch (WebSocketException wex)
            {
                // Only fire failure if we didn't already get an ERROR message
                TunnelFailed?.Invoke(this, $"WebSocket error: {wex.Message}");
                _connectTcs?.TrySetResult(false);
            }
            catch (Exception ex)
            {
                TunnelFailed?.Invoke(this, $"Unexpected error: {ex.Message}");
                _connectTcs?.TrySetResult(false);
            }
        }

        private async Task HandleMessageAsync(string message)
        {
            using var doc = JsonDocument.Parse(message);

            if (doc.RootElement.TryGetProperty("Type", out var type))
            {
                switch (type.GetString())
                {
                    case "TUNNEL_CLOSED":
                        TunnelDeactivated?.Invoke(this, EventArgs.Empty);
                        return;

                    case "ERROR":
                        TunnelFailed?.Invoke(this, message);
                        _connectTcs?.TrySetResult(false);
                        return;
                }
            }

            // HTTP or tunnel establish message
            //Console.WriteLine("CLI: Received raw message: " + message.Substring(0, Math.Min(150, message.Length)) + "...");

            var tunnelInfo = JsonSerializer.Deserialize<TunnelCreateResponse>(message);
            if (tunnelInfo != null && !string.IsNullOrEmpty(tunnelInfo.PublicUrl))
            {
                TunnelEstablished?.Invoke(this, tunnelInfo);
                _connectTcs?.TrySetResult(true);
                return;
            }

            MessageReceived?.Invoke(this, message);
            await ForwardToLocalhost(message);
        }

        private async Task ForwardToLocalhost(string httpRequestJson)
        {
            var request = JsonSerializer.Deserialize<HttpRequestData>(httpRequestJson);
            if (request == null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Failed to deserialize request");
                Console.ResetColor();
                return;
            }

            var baseUrl = _configuration["LocalServer:BaseUrl"] ?? "http://localhost";
            var fullBase = $"{baseUrl}:{_localPort}";

            using var httpClient = new HttpClient { BaseAddress = new Uri(fullBase) };

            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod(request.Method),
                RequestUri = new Uri(request.Path, UriKind.Relative)
            };

            foreach (var header in request.Headers)
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);

            if (!string.IsNullOrEmpty(request.Body))
                httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");

            try
            {
                var response = await httpClient.SendAsync(httpRequest);
                var body = await response.Content.ReadAsStringAsync();

                // Determine emoji/color based on status code
                string emoji;
                ConsoleColor color;
                if ((int)response.StatusCode >= 200 && (int)response.StatusCode < 300)
                {
                    //emoji = "✅"; color = ConsoleColor.Green;
                    emoji = "🌐"; color = ConsoleColor.Gray;
                }
                else if ((int)response.StatusCode == 401)
                {
                    emoji = "🔒"; color = ConsoleColor.Gray;
                }
                else if ((int)response.StatusCode == 404)
                {
                    emoji = "❌"; color = ConsoleColor.Gray;
                }
                else
                {
                    emoji = "💥"; color = ConsoleColor.Gray;
                }

                Console.ForegroundColor = color;
                Console.WriteLine($"{emoji} {request.Method} {request.Path} → {response.StatusCode}");
                Console.ResetColor();

                var responseData = new HttpResponseData
                {
                    RequestId = request.RequestId,
                    StatusCode = (int)response.StatusCode,
                    Body = body,
                    Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value))
                };

                var responseJson = JsonSerializer.Serialize(responseData);
                await SendAsync(responseJson);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"💥 {request.Method} {request.Path} → Forward failed: {ex.Message}");
                Console.ResetColor();
            }
        }

        public async Task SendAsync(string message)
        {
            if (_websocket?.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _websocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_websocket?.State == WebSocketState.Open)
            {
                await _websocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "User disconnected",
                    CancellationToken.None);
            }
            TunnelClosed?.Invoke(this, EventArgs.Empty);
        }

        public async Task DeactivateTunnel(Guid TunnelId)
        {
            var context = new HttpClient();
            var serverUrl = _configuration["RelayServer:Url"];
            var response = await context.DeleteAsync($"{serverUrl}/api/tunnel/deactivate/{TunnelId}");
            response.EnsureSuccessStatusCode();
            TunnelDeactivated?.Invoke(this, EventArgs.Empty);
        }

        private bool TryParseControlMessage(string message, out string type, out string? reason)
        {
            type = "";
            reason = null;

            try
            {
                using var doc = JsonDocument.Parse(message);
                if (doc.RootElement.TryGetProperty("Type", out var typeProp))
                {
                    type = typeProp.GetString() ?? "";
                    doc.RootElement.TryGetProperty("Reason", out var reasonProp);
                    reason = reasonProp.GetString();
                    return true;
                }
            }
            catch { }

            return false;
        }

    }
}


