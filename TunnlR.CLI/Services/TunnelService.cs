using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using TunnlR.Application.DTOs.Tunnel;

namespace TunnlR.CLI.Services
{
    public class TunnelService
    {
        private readonly IConfiguration _configuration;
        private ClientWebSocket? _websocket;

        public event EventHandler<string>? MessageReceived;
        public event EventHandler<TunnelCreateResponse>? TunnelEstablished;
        public event EventHandler? TunnelClosed;


        public TunnelService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ConnectAsync(string token, int localPort, string protocol)
        {
            var wsUrl = _configuration["RelayServer:WebSocketUrl"]!;

            _websocket = new ClientWebSocket();

            var uri = new Uri($"{wsUrl}/tunnel?token={token}&port={localPort}&protocol={protocol}");
            await _websocket.ConnectAsync(uri, CancellationToken.None);

            Console.WriteLine("✅ Connected to relay server!");

            // Start listening in background
             _ = Task.Run(ListenAsync);
        }

        private async Task ListenAsync()
        {
            var buffer = new byte[1024 * 4]; 

            while (_websocket?.State == WebSocketState.Open)
            {
                var result = await _websocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _websocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);

                    TunnelClosed?.Invoke(this, EventArgs.Empty);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    await HandleMessageAsync(message);
                }
            }
        }

        private async Task HandleMessageAsync(string message)
        {
            try
            {
                var tunnelInfo = JsonSerializer.Deserialize<TunnelCreateResponse>(message);

                if (tunnelInfo != null && !string.IsNullOrEmpty(tunnelInfo.PublicUrl))
                {
                    TunnelEstablished?.Invoke(this, tunnelInfo);
                }
                else
                {
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch
            {
                MessageReceived?.Invoke(this, message);
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
    }
}
