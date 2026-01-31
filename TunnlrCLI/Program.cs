using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using Tunnlr.Services;  
using Application.Persistence;
using Microsoft.Extensions.Configuration;
using Application.Helper;

class Program
{
    static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

       
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? "Data Source=tunnels.db"; 

        builder.Services.AddDbContext<TunnelDbContext>(options =>
            options.UseSqlite(connectionString));

        builder.Services.AddSingleton<TunnelService>();  

        using var host = builder.Build();

        await RunCliLoopAsync(host.Services);
    }

    private static async Task RunCliLoopAsync(IServiceProvider services)
    {
        var tunnelService = services.GetRequiredService<TunnelService>();

        Console.Clear();
        Helpers.PrintAnimatedHeader("🚀 Tunnlr CLI", ConsoleColor.Cyan, 50);
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
                Helpers.PrintHelp();
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
                Helpers.AnimateDots(6, 300);
                Console.WriteLine();
                Helpers.PrintLoadingBar("Initializing services", 20, 30);

                var tunnel = tunnelService.StartTunnel(port, protocol);

                using var scope = services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<TunnelDbContext>();

                Helpers.PrintTunnelInfo(tunnel);
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
}