using TunnlR.CLI.Services;
namespace TunnlR.CLI.Commands
{
    public class DeactivateTunnelCommand
    {
        private readonly CLITunnelService _clitunnelService;
         
        public DeactivateTunnelCommand(CLITunnelService clitunnelService)
        {
            _clitunnelService = clitunnelService;
        }

        public async Task ExecuteAsync(Guid TunnelId)
        {
            if (TunnelId == Guid.Empty)
            {
                Console.WriteLine("Please, provide a TunnelId");
            }
            await _clitunnelService.DeactivateTunnel(TunnelId);

            _clitunnelService.TunnelDeactivated += OnTunnelDeactivated;
        }

        private void OnTunnelDeactivated(object? sender, EventArgs e)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ Tunnel deactivated!");
            Console.ResetColor();
        }
    }
}
