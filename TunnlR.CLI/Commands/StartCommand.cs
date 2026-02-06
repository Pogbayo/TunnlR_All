using TunnlR.CLI.Configuration;
using TunnlRCLI.Helpers;
using TunnlR.CLI.Services;
using TunnlR.Contract.DTOs.Auth;
using TunnlR.Contract.DTOs.TunnelDto;
namespace TunnlR.CLI.Commands
{
    public class StartCommand
    {
        private readonly CLITunnelService _clitunnelService;
        private readonly TokenStorage _tokenStorage;

        public StartCommand(CLITunnelService clitunnelService, TokenStorage tokenStorage)
        {
            _clitunnelService = clitunnelService;
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

            await _clitunnelService.ConnectAsync(token, port, protocol);

            _clitunnelService.TunnelEstablished += OnTunnelEstablished;
            _clitunnelService.MessageReceived += OnMessageReceived;

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
