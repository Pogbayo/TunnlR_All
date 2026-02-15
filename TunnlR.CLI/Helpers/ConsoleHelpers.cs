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

        // Modern dots animation
        public static async Task PrintLoadingBarAsync(string message, int steps = 20, int delayMs = 50)
        {
            var frames = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
            var colors = new[] { ConsoleColor.Cyan, ConsoleColor.Blue, ConsoleColor.DarkCyan };

            Console.Write(message + " ");
            var startPos = Console.CursorLeft;

            for (int i = 0; i < steps; i++)
            {
                foreach (var frame in frames)
                {
                    Console.SetCursorPosition(startPos, Console.CursorTop);
                    Console.ForegroundColor = colors[i % colors.Length];
                    Console.Write(frame);
                    await Task.Delay(delayMs);
                }
            }

            Console.SetCursorPosition(startPos, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✓");
            Console.ResetColor();
        }

        // Alternative: Progress bar style
        public static async Task PrintProgressBarAsync(string message, int steps = 30, int delayMs = 50)
        {
            Console.Write(message + " ");
            Console.Write("[");
            var barStart = Console.CursorLeft;
            Console.Write(new string(' ', steps));
            Console.Write("]");

            for (int i = 0; i < steps; i++)
            {
                Console.SetCursorPosition(barStart + i, Console.CursorTop);
                Console.ForegroundColor = i < steps / 3 ? ConsoleColor.Red :
                                         i < steps * 2 / 3 ? ConsoleColor.Yellow :
                                         ConsoleColor.Green;
                Console.Write("█");
                await Task.Delay(delayMs);
            }

            Console.SetCursorPosition(0, Console.CursorTop + 1);
            Console.ResetColor();
        }

        // Tunnel info with async effects
        public static async Task PrintTunnelInfoAsync(TunnelCreateResponse tunnel)
        {
            Console.WriteLine();

            // Success checkmark animation
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("✓ ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Tunnel started");
            await Task.Delay(200);

            Console.WriteLine();

            // Public URL with box
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("┌─────────────────────────────────────────────────");
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Public URL  ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(tunnel.PublicUrl);
            await Task.Delay(150);

            // Dashboard URL
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("│ ");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Dashboard   ");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine(tunnel.DashboardUrl);
            await Task.Delay(150);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("└─────────────────────────────────────────────────");

            Console.ResetColor();
        }

        public static void PrintHelp()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n━━━ Commands ━━━");
            Console.ResetColor();

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  start");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine(" --port 5000 --protocol http");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("    Start a new tunnel\n");

            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("  exit");
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("\n    Exit the CLI\n");

            Console.ResetColor();
        }

        public static void PrintTunnelConnectionFailed(string errorMessage)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n✗ Tunnel failed: {errorMessage}");
            Console.ResetColor();
        }
    }
}