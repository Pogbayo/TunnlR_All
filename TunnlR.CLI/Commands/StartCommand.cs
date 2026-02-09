using TunnlR.CLI.Configuration;
using TunnlRCLI.Helpers;
using TunnlR.CLI.Services;
using TunnlR.Contract.DTOs.TunnelDto;
namespace TunnlR.CLI.Commands
{
    public class StartCommand
    {
        private readonly CLITunnelService _cliTunnelService;
        private readonly TokenStorage _tokenStorage;

        public StartCommand(CLITunnelService cliTunnelService, TokenStorage tokenStorage)
        {
            _cliTunnelService = cliTunnelService;
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

            var tunnelEstablishedTcs = new TaskCompletionSource<bool>();

            _cliTunnelService.TunnelEstablished += (s, tunnelInfo) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✅ Tunnel started!");
                Console.WriteLine($"Public URL: {tunnelInfo.PublicUrl}");
                Console.WriteLine($"Dashboard: {tunnelInfo.DashboardUrl}");
                Console.ResetColor();
                tunnelEstablishedTcs.TrySetResult(true);
            };

            _cliTunnelService.TunnelFailed += (s, errorMessage) =>
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"\n❌ Tunnel failed: {errorMessage}");
                Console.ResetColor();
                tunnelEstablishedTcs.TrySetResult(false); 
            };

            _cliTunnelService.MessageReceived += (s, message) =>
            {
                Console.WriteLine($"📨 Message: {message}");
            };

            await _cliTunnelService.ConnectAsync(token, port, protocol);

            var success = await tunnelEstablishedTcs.Task;

            if (success)
            {
                Console.WriteLine("\nPress Ctrl+C to stop the tunnel...");
                await Task.Delay(Timeout.Infinite); 
            }
            else
            {
                Console.WriteLine("Tunnel could not be established. Exiting CLI.");
            }
        }
    }
}

 