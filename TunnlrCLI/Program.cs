using Tunnlr.Services;

class Program
{
    static void Main(string[] args)
    {
        var tunnelService = new TunnelService();

        Console.Clear();
        PrintAnimatedHeader("🚀 Tunnlr CLI", ConsoleColor.Cyan, 50);
        Console.WriteLine("Type 'help' to see commands.\n");

        while (true)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("> ");
            Console.ResetColor();

            string input = Console.ReadLine() ?? "";

            if (string.IsNullOrWhiteSpace(input)) continue;

            string command = input.Trim().ToLower();

            if (command == "exit") break;

            if (command == "help")
            {
                PrintHelp();
                continue;
            }

            if (command.StartsWith("start"))
            {
                int port = 5000;
                string protocol = "http";

                string[] parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < parts.Length; i++)
                {
                    switch (parts[i])
                    {
                        case "--port":
                            port = int.Parse(parts[i + 1]);
                            i++;
                            break;
                        case "--protocol":
                            protocol = parts[i + 1];
                            i++;
                            break;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write("\nStarting tunnel");
                AnimateDots(6, 300);  
                Console.WriteLine();

                PrintLoadingBar("Initializing services", 20, 30);

                var tunnel = tunnelService.StartTunnel(port, protocol);

                PrintTunnelInfo(tunnel);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("❌ Unknown command. Type 'help'.\n");
                Console.ResetColor();
            }
        }

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\nExiting Tunnlr CLI... Bye! 👋");
        Console.ResetColor();
    }

    static void PrintHelp()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\nCommands:");
        Console.WriteLine("start --port 5000 --protocol http  : Start a new tunnel");
        Console.WriteLine("exit                                : Exit the CLI\n");
        Console.ResetColor();
    }

    static void PrintTunnelInfo(dynamic tunnel)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("\n✅ Tunnel started!\n");

        Thread.Sleep(500);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"Local Port   : {tunnel.LocalPort}");
        Thread.Sleep(300);

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Public URL   : {tunnel.PublicUrl}");
        Thread.Sleep(300);

        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine($"Dashboard URL: {tunnel.DashboardUrl}\n");
        Console.ResetColor();
    }

    static void AnimateDots(int count, int delay)
    {
        for (int i = 0; i < count; i++)
        {
            Console.Write(".");
            Thread.Sleep(delay);
        }
    }

    static void PrintLoadingBar(string message, int steps, int delay)
    {
        Console.Write(message + " [");
        for (int i = 0; i < steps; i++)
        {
            Console.Write("=");
            Thread.Sleep(delay);
        }
        Console.WriteLine("]");
    }

    static void PrintAnimatedHeader(string text, ConsoleColor color, int charDelay)
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
}
