using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TunnlR.CLI.Commands;
using TunnlR.CLI.Configuration;
using TunnlR.CLI.Services;
using TunnlRCLI.Helpers;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddHttpClient<AuthenticationService>();
builder.Services.AddSingleton<CLITunnelService>();
builder.Services.AddSingleton<TokenStorage>();
builder.Services.AddSingleton<LoginCommand>();
builder.Services.AddSingleton<SignUpCommand>();  
builder.Services.AddSingleton<StartCommand>();
builder.Services.AddSingleton<StopCommand>();

var host = builder.Build();

ConsoleHelpers.PrintAnimatedHeader("🚀 TunnlR CLI", ConsoleColor.Cyan, 50);
ConsoleHelpers.PrintHelp();

while (true)
{
    Console.Write("> ");
    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input)) continue;

    var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    var command = parts[0].ToLower();

    try
    {
        switch (command)
        {
            case "login":
                var loginCmd = host.Services.GetRequiredService<LoginCommand>();
                Console.Write("Email: ");
                var email = Console.ReadLine();
                Console.Write("Password: ");
                var password = ReadPassword();
                await loginCmd.ExecuteAsync(email!, password);
                break;

            case "register":
                var registerCmd = host.Services.GetRequiredService<SignUpCommand>();
                Console.Write("Email: ");
                var regEmail = Console.ReadLine();
                Console.Write("Password: ");
                var regPassword = ReadPassword();
                Console.Write("Confirm Password: ");
                var confirmPassword = ReadPassword();
                await registerCmd.ExecuteAsync(regEmail!, regPassword, confirmPassword);
                break;

            case "start":
                var startCmd = host.Services.GetRequiredService<StartCommand>();
                var portIndex = Array.IndexOf(parts, "--port");
                if (portIndex == -1 || portIndex + 1 >= parts.Length)
                {
                    Console.WriteLine("Usage: start --port <port>");
                    break;
                }
                var port = int.Parse(parts[portIndex + 1]);
                await startCmd.ExecuteAsync(port);
                break;

            case "help":
                ConsoleHelpers.PrintHelp();
                break;

            case "exit":
                return;

            default:
                Console.WriteLine($"Unknown command: {command}");
                ConsoleHelpers.PrintHelp();
                break;
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}

static string ReadPassword()
{
    var password = string.Empty;
    ConsoleKey key;
    do
    {
        var keyInfo = Console.ReadKey(intercept: true);
        key = keyInfo.Key;

        if (key == ConsoleKey.Backspace && password.Length > 0)
        {
            Console.Write("\b \b");
            password = password[0..^1];
        }
        else if (!char.IsControl(keyInfo.KeyChar))
        {
            Console.Write("*");
            password += keyInfo.KeyChar;
        }
    } while (key != ConsoleKey.Enter);

    Console.WriteLine();
    return password;
}