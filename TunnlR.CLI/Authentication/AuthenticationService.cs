
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using TunnlR.Application.DTOs.Auth;

public class AuthenticationService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    
    public AuthenticationService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        
        var serverUrl = _configuration["RelayServer:Url"] 
            ?? throw new InvalidOperationException("RelayServer:Url not configured");
        _httpClient.BaseAddress = new Uri(serverUrl);
    }
    
    public async Task<LoginResponse> LoginAsync(string email, string password)
    {
        var request = new LoginRequest 
        { 
            Email = email, 
            Password = password 
        };
        
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return result ?? throw new Exception("Failed to deserialize login response");
    }

    public async Task<RegisterResponse> RegisterAsync(string email, string password, string confirmPassword)
    {
        var request = new RegisterRequest
        {
            Email = email,
            Password = password,
            ConfirmPassword = confirmPassword
        };

        var response = await _httpClient.PostAsJsonAsync("/api/auth/register", request);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        return result ?? throw new Exception("Failed to deserialize register response");
    }
}
}
