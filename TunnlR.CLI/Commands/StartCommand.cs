using TunnlR.CLI.Configuration;
using TunnlR.CLI.Services;
using TunnlR.Contract.DTOs.TunnelDto;
using TunnlR.Domain.Entities;
using TunnlRCLI.Helpers;
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
            await ConsoleHelpers.PrintLoadingBarAsync("Connecting", 20, 50);

            var tunnelEstablishedTcs = new TaskCompletionSource<bool>();

            //Subscribing and unsubscribing events from the server to avoid work load
            EventHandler<TunnelCreateResponse>? onEstablished = null;
            EventHandler<string>? onFailed = null;

            onEstablished = async (s, tunnelInfo) =>
            {
                await ConsoleHelpers.PrintTunnelInfoAsync(tunnelInfo);
                tunnelEstablishedTcs.TrySetResult(true);
            };

            onFailed = (s, errorMessage) =>
            {
                ConsoleHelpers.PrintTunnelConnectionFailed(errorMessage);
                tunnelEstablishedTcs.TrySetResult(false);
            };

            _cliTunnelService.TunnelEstablished += onEstablished;
            _cliTunnelService.TunnelFailed += onFailed;

            await _cliTunnelService.ConnectAsync(token, port, protocol);

            var success = await tunnelEstablishedTcs.Task;

            _cliTunnelService.TunnelEstablished -= onEstablished;
            _cliTunnelService.TunnelFailed -= onFailed;

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

 