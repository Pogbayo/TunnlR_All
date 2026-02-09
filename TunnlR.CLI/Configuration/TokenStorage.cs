namespace TunnlR.CLI.Configuration
{
    public class TokenStorage
    {
        private readonly string _tokenFilePath;

        public TokenStorage()
        {
            // Gets the path to the user's AppData/Roaming directory (e.g., C:\Users\<User>\AppData\Roaming)
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            //Creates a path for a .tunnlr folder inside the AppData directory
            var tunnlrDir = Path.Combine(appDataPath, ".tunnlr");
            //Creates the .tunnlr directory if it doesn't exist
            Directory.CreateDirectory(tunnlrDir);
            //Sets the path for a token.txt file inside the .tunnlr directory
            _tokenFilePath = Path.Combine(tunnlrDir, "token.txt");
        }

        public async Task SaveTokenAsync(string token)
        {
            DeleteToken();
            await File.WriteAllTextAsync(_tokenFilePath, token);
        }

        public async Task<string?> GetTokenAsync()
        {
            if (!File.Exists(_tokenFilePath))
                return null;

            return await File.ReadAllTextAsync(_tokenFilePath);
        }

        public void DeleteToken()
        {
            if (File.Exists(_tokenFilePath))
                File.Delete(_tokenFilePath);
        }
    }
}
 