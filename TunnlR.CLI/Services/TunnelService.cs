using System.Net.WebSockets;
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
    }
}
