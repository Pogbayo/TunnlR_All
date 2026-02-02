using TunnlR.Application.DTOs.Tunnel;
using TunnlR.Application.Services.TunnelServices;
using TunnlR.CLI.Configuration;
using TunnlRCLI.Helpers;

namespace TunnlR.CLI.Commands
{
    public class StartCommand
    {
        private readonly TunnelService _tunnelService;
        private readonly TokenStorage _tokenStorage;

        public StartCommand(TunnelService tunnelService, TokenStorage tokenStorage)
        {
            _tunnelService = tunnelService;
            _tokenStorage = tokenStorage;
        }

        public async Task ExecuteAsync(int port, string protocol = "http")
        {
            var token = await _tokenStorage.GetTokenAsync();

            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("❌ Not logged in. Run 'tunnlr login' first.");
                return;
            }

            Console.WriteLine($"Starting tunnel on port {port}...");
            ConsoleHelpers.PrintLoadingBar("Connecting", 20, 50);

            _tunnelService.TunnelEstablished += OnTunnelEstablished;
            _tunnelService.MessageReceived += OnMessageReceived;

            await _tunnelService.ConnectAsync(token, port, protocol);

            Console.WriteLine("\nPress Ctrl+C to stop the tunnel...");
            await Task.Delay(Timeout.Infinite);
        }
        private void OnTunnelEstablished(object? sender, TunnelCreateResponse tunnelInfo)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✅ Tunnel started!");
            Console.WriteLine($"Public URL: {tunnelInfo.PublicUrl}");
            Console.WriteLine($"Dashboard: {tunnelInfo.DashboardUrl}");
            Console.ResetColor();
        }


        private void OnMessageReceived(object? sender, string message)
        {
            Console.WriteLine($"📨 Message: {message}");
        }
    }
}
