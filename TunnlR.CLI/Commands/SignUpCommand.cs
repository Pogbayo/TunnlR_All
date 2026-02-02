namespace TunnlR.CLI.Commands
{
    public class SignUpCommand
    {
        private readonly AuthenticationService _authService;

        public SignUpCommand(AuthenticationService authService)
        {
            _authService = authService;
        }

        public async Task ExecuteAsync(string email, string password, string confirmPassword)
        {
            try
            {
                Console.WriteLine("Registering...");

                var response = await _authService.RegisterAsync(email, password, confirmPassword);

                if (response.Success)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("✅ Registration successful! You can now login.");
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"❌ Registration failed: {response.Message}");
                }

                Console.ResetColor();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.ResetColor();
            }
        }
    }
}
