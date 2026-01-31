using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Entities;

namespace TunnlRCLI.Helpers
{
    public class ConsoleHelpers
    {
        public static void PrintAnimatedHeader(string text, ConsoleColor color, int charDelay)
        {
            Console.ForegroundColor = color;
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(charDelay);
            }
            Console.WriteLine("\n");
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

        public static void PrintTunnelInfo(Tunnel tunnel)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n✅ Tunnel started!\n");
            Thread.Sleep(500);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Local Port : {tunnel.LocalPort}");
            Thread.Sleep(300);

            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine($"Public URL : {tunnel.PublicUrl}");
            Thread.Sleep(300);

            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"Dashboard URL: {tunnel.DashboardUrl}\n");
            Console.ResetColor();
        }

        public static void AnimateDots(int count, int delay)
        {
            for (int i = 0; i < count; i++)
            {
                Console.Write(".");
                Thread.Sleep(delay);
            }
        }

        public static void PrintLoadingBar(string message, int steps, int delay)
        {
            Console.Write(message + " [");
            for (int i = 0; i < steps; i++)
            {
                Console.Write("=");
                Thread.Sleep(delay);
            }
            Console.WriteLine("]");
        }
    }
}
