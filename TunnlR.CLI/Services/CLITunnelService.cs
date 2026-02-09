using System;
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

        public CLITunnelService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ConnectAsync(string token, int localPort, string protocol)
        {
            _localPort = localPort;
            var wsUrl = _configuration["RelayServer:WebSocketUrl"]!;
            _websocket = new ClientWebSocket();

            var uri = new Uri($"{wsUrl}/tunnel?token={token}&port={localPort}&protocol={protocol}");
            try
            {
                await _websocket.ConnectAsync(uri, CancellationToken.None);
                Console.WriteLine("WebSocket connected to relay!");
            }
            catch (Exception ex)
            {
                TunnelFailed?.Invoke(this, $"Failed to connect to relay server: {ex.Message}");
                return;
            }

            _ = Task.Run(ListenAsync);
        }

        private async Task ListenAsync()
        {
            var buffer = new byte[1024 * 4];

            while (_websocket?.State == WebSocketState.Open)
            {
                var result = await _websocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // If the server returns an error message
                if (message.Contains("error", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("Exception") ||
                    message.Contains("failed", StringComparison.OrdinalIgnoreCase))
                {
                    TunnelFailed?.Invoke(this, message);
                    continue;
                }

                await HandleMessageAsync(message);
            }
        }

        private async Task HandleMessageAsync(string message)
        {
            try
            {
                Console.WriteLine("CLI: Received raw message: " + message.Substring(0, Math.Min(150, message.Length)) + "...");
                var tunnelInfo = JsonSerializer.Deserialize<TunnelCreateResponse>(message);

                if (tunnelInfo != null && !string.IsNullOrEmpty(tunnelInfo.PublicUrl))
                {
                    Console.WriteLine("CLI: Received tunnel establishment response");
                    TunnelEstablished?.Invoke(this, tunnelInfo);
                    return;                   
                }
               Console.WriteLine("CLI: This is an HTTP request → forwarding to localhost");
                await ForwardToLocalhost(message);
                MessageReceived?.Invoke(this, message);
            }
            catch(Exception ex)
            {
                Console.WriteLine("CLI: Error in HandleMessageAsync: " + ex.Message);
                try { await ForwardToLocalhost(message); } catch { }
            }
        }

        private async Task ForwardToLocalhost(string httpRequestJson)
        {
            Console.WriteLine("CLI: Received request JSON: " + httpRequestJson);

            var request = JsonSerializer.Deserialize<HttpRequestData>(httpRequestJson);
            if (request == null)
            {
                Console.WriteLine("CLI: Failed to deserialize request");
                return;
            }

            // Strip test prefix for local testing
            var localPath = request.Path;
            if (localPath.StartsWith("/api/tunnel/"))
            {
                var idEndIndex = localPath.IndexOf('/', "/api/tunnel/".Length + 36);
                if (idEndIndex > 0)
                {
                    localPath = localPath.Substring(idEndIndex);
                }
                else
                {
                    localPath = "/";
                }
                Console.WriteLine("CLI: Stripped test path → forwarding to: " + localPath);
            }

            var baseUrl = _configuration["LocalServer:BaseUrl"] ?? "http://localhost";
            var fullBase = $"{baseUrl}:{_localPort}";
            Console.WriteLine($"CLI: Forwarding {request.Method} to {fullBase}{localPath}");

            using var httpClient = new HttpClient { BaseAddress = new Uri(fullBase) };

            var httpRequest = new HttpRequestMessage
            {
                Method = new HttpMethod(request.Method),
                RequestUri = new Uri(localPath, UriKind.Relative)
            };

            // Copy headers
            foreach (var header in request.Headers)
            {
                httpRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            // Copy body if present
            if (!string.IsNullOrEmpty(request.Body))
            {
                httpRequest.Content = new StringContent(request.Body, Encoding.UTF8, "application/json");
            }

            try
            {
                var response = await httpClient.SendAsync(httpRequest);
                Console.WriteLine($"CLI: Local response status: {response.StatusCode}");

                var body = await response.Content.ReadAsStringAsync();
                Console.WriteLine("CLI: Local response body: " + body);

                var responseData = new HttpResponseData
                {
                    RequestId = request.RequestId,
                    StatusCode = (int)response.StatusCode,
                    Body = body,
                    Headers = response.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value))
                };

                var responseJson = JsonSerializer.Serialize(responseData);
                await SendAsync(responseJson);
                Console.WriteLine("CLI: Successfully sent response back to relay");
            }
            catch (Exception ex)
            {
                Console.WriteLine("CLI: Forward failed: " + ex.Message);
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
        }
    }
}
