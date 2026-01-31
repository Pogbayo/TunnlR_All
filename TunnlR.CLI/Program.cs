using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TunnlR.CLI.Configuration;

//using Ap.Services;  
using TunnlRCLI.Helpers;

class Program
{

    static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.json", optional: true);
        builder.Services.AddHttpClient<AuthenticationService>();

        builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

        using var host = builder.Build();

         RunCliLoopAsync(host.Services);
    }

    private static void RunCliLoopAsync(IServiceProvider services)
    {
        //var tunnelService = services.GetRequiredService<TunnelService>();

        Console.Clear();
        ConsoleHelpers.PrintAnimatedHeader("🚀 Tunnlr CLI", ConsoleColor.Cyan, 50);
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
                ConsoleHelpers.PrintHelp();
                continue;
            }
            if (command.StartsWith("tunnlr start"))
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
                ConsoleHelpers.AnimateDots(6, 300);
                Console.WriteLine();
                ConsoleHelpers.PrintLoadingBar("Initializing services", 20, 30);

                //var tunnel = tunnelService.StartTunnel(port, protocol);

                //using var scope = services.CreateScope();
                //var db = scope.ServiceProvider.GetRequiredService<TunnelDbContext>();

                //ConsoleHelpers.PrintTunnelInfo(tunnel);
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