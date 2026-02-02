
using TunnlR.CLI.Configuration;

namespace TunnlR.CLI.Commands
{
    public class LoginCommand
    {
        private readonly AuthenticationService _authService;
        private readonly TokenStorage _tokenStorage;

        public LoginCommand(AuthenticationService authService, TokenStorage tokenStorage)
        {
            _authService = authService;
            _tokenStorage = tokenStorage;
        }

        public async Task ExecuteAsync(string email, string password)
        {
            try
            {
                if (String.IsNullOrEmpty(email))
                {
                    Console.WriteLine("Please enter your email");
                }

                if (String.IsNullOrEmpty(password))
                {
                    Console.WriteLine("Please enter your password");
                }

                Console.WriteLine("Logging in...");

                var response = await _authService.LoginAsync(email, password);
                await _tokenStorage.SaveTokenAsync(response.Token);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Login successful!");
                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Login failed: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
