using TunnlR.Contract.DTOs.TunnelDto;
using TunnlR.Domain.Entities;

namespace TunnlRCLI.Helpers
{
    public static class ConsoleHelpers
    {
        // Animated header
        public static async Task PrintAnimatedHeaderAsync(string text, ConsoleColor color, int charDelay = 50)
        {
            Console.ForegroundColor = color;
            foreach (char c in text)
            {
                Console.Write(c);
                await Task.Delay(charDelay);
            }
            Console.WriteLine("\n");
            Console.ResetColor();
        }

        // Spinner animation for async tasks
        public static async Task SpinnerAsync(string message, int durationMs = 2000)
        {
            var spinner = new[] { '|', '/', '-', '\\' };
            var end = DateTime.Now.AddMilliseconds(durationMs);
            int i = 0;

            Console.Write(message + " ");
            while (DateTime.Now < end)
            {
                Console.Write(spinner[i % spinner.Length]);
                await Task.Delay(100);
                Console.Write("\b");
                i++;
            }
            Console.WriteLine();
        }

        // Async loading bar
        public static async Task PrintLoadingBarAsync(string message, int steps = 30, int delayMs = 50)
        {
            Console.Write(message + " [");
            for (int i = 0; i < steps; i++)
            {
                Console.Write("=");
                await Task.Delay(delayMs);
            }
            Console.WriteLine("]");
        }

        // Tunnel info with async effects
        public static async Task PrintTunnelInfoAsync(TunnelCreateResponse tunnel)
        {
            // Title
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\nTunnel started\n");
            await Task.Delay(300);

            // Public URL
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Public URL : ");
            Console.ForegroundColor = ConsoleColor.Cyan; 
            Console.WriteLine(tunnel.PublicUrl);
            await Task.Delay(200);

            // Dashboard URL
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Dashboard  : ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(tunnel.DashboardUrl);
            await Task.Delay(200);

            Console.ResetColor();
        }


        public static void PrintHelp()
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nCommands:");
            Console.WriteLine("start --port 5000 --protocol http : Start a new tunnel");
            Console.WriteLine("exit : Exit the CLI\n");
            Console.ResetColor();
        }

        public static void PrintTunnelConnectionFailed(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ Tunnel failed: {errorMessage}");
            Console.ResetColor();
        }
    }
}
