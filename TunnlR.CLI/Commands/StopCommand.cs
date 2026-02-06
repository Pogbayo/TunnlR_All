using TunnlR.CLI.Services;

namespace TunnlR.CLI.Commands
{
    public class StopCommand
    {
        private readonly CLITunnelService _tunnelService;

        public StopCommand(CLITunnelService tunnelService)
        {
            _tunnelService = tunnelService;
        }

        public async Task ExecuteAsync()
        {
            _tunnelService.TunnelClosed += OnTunnelClosed;

            Console.WriteLine("Stopping tunnel...");

            // Disconnect from server
            await _tunnelService.DisconnectAsync();

            // Wait a moment for the event to fire
            await Task.Delay(500);
        }

        // Event handler - runs when TunnelClosed event is invoked
        private void OnTunnelClosed(object? sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("✅ Tunnel stopped successfully");
            Console.ResetColor();
        }
    }
}
