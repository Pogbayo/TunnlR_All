TunnlR Project - Complete Implementation Checklist

1. TunnlR.Domain Layer
Purpose: Shared entities and contracts (DTOs)
Folders & Files:
TunnlR.Domain/
├── Entities/
│   ├── AppUser.cs
│   ├── Tunnel.cs
│   └── TunnelSession.cs
│
└── DTOs/
    ├── Auth/
    │   ├── LoginRequest.cs
    │   ├── LoginResponse.cs
    │   ├── RegisterRequest.cs
    │   └── RegisterResponse.cs
    │
    └── Tunnel/
        ├── TunnelCreateRequest.cs
        ├── TunnelCreateResponse.cs
        └── TunnelStatusResponse.cs
What to do:
Entities/AppUser.cs:
csharpusing Microsoft.AspNetCore.Identity;

namespace TunnlR.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {
        public DateTime CreatedAt { get; set; }
        public ICollection<Tunnel> Tunnels { get; set; } = new List<Tunnel>();
    }
}
Entities/Tunnel.cs:
csharpnamespace TunnlR.Domain.Entities
{
    public class Tunnel
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string PublicUrl { get; set; } = string.Empty;
        public string DashboardUrl { get; set; } = string.Empty;
        public int LocalPort { get; set; }
        public string Protocol { get; set; } = "http"; // http or https
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        
        public AppUser User { get; set; } = null!;
    }
}
Entities/TunnelSession.cs:
csharpnamespace TunnlR.Domain.Entities
{
    public class TunnelSession
    {
        public Guid Id { get; set; }
        public Guid TunnelId { get; set; }
        public string ConnectionId { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public long BytesTransferred { get; set; }
        
        public Tunnel Tunnel { get; set; } = null!;
    }
}
DTOs/Auth/LoginRequest.cs:
csharpnamespace TunnlR.Domain.DTOs.Auth
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
DTOs/Auth/LoginResponse.cs:
csharpnamespace TunnlR.Domain.DTOs.Auth
{
    public class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
    }
}
DTOs/Auth/RegisterRequest.cs:
csharpnamespace TunnlR.Domain.DTOs.Auth
{
    public class RegisterRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
DTOs/Auth/RegisterResponse.cs:
csharpnamespace TunnlR.Domain.DTOs.Auth
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
DTOs/Tunnel/TunnelCreateRequest.cs:
csharpnamespace TunnlR.Domain.DTOs.Tunnel
{
    public class TunnelCreateRequest
    {
        public int LocalPort { get; set; }
        public string Protocol { get; set; } = "http";
    }
}
DTOs/Tunnel/TunnelCreateResponse.cs:
csharpnamespace TunnlR.Domain.DTOs.Tunnel
{
    public class TunnelCreateResponse
    {
        public Guid TunnelId { get; set; }
        public string PublicUrl { get; set; } = string.Empty;
        public string DashboardUrl { get; set; } = string.Empty;
    }
}
DTOs/Tunnel/TunnelStatusResponse.cs:
csharpnamespace TunnlR.Domain.DTOs.Tunnel
{
    public class TunnelStatusResponse
    {
        public Guid TunnelId { get; set; }
        public bool IsActive { get; set; }
        public long BytesTransferred { get; set; }
        public int RequestCount { get; set; }
    }
}
```

---

## **2. TunnlR.Infrastructure Layer**

### Purpose: Database, Identity, and external services

### Folders & Files:
```
TunnlR.Infrastructure/
├── Persistence/
│   ├── TunnelDbContext.cs
│   └── Migrations/ (generated)
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs
│
└── TokenAuthentication/
    ├── JwtTokenGenerator.cs
    └── IJwtTokenGenerator.cs
What to do:
Persistence/TunnelDbContext.cs:
csharpusing Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TunnlR.Domain.Entities;

namespace TunnlR.Infrastructure.Persistence
{
    public class TunnelDbContext : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>
    {
        public TunnelDbContext(DbContextOptions<TunnelDbContext> options) 
            : base(options) { }

        public DbSet<Tunnel> Tunnels { get; set; }
        public DbSet<TunnelSession> TunnelSessions { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Tunnel>()
                .HasOne(t => t.User)
                .WithMany(u => u.Tunnels)
                .HasForeignKey(t => t.UserId);

            builder.Entity<TunnelSession>()
                .HasOne(ts => ts.Tunnel)
                .WithMany()
                .HasForeignKey(ts => ts.TunnelId);
        }
    }
}
TokenAuthentication/IJwtTokenGenerator.cs:
csharpusing TunnlR.Domain.Entities;

namespace TunnlR.Infrastructure.TokenAuthentication
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(AppUser user);
    }
}
TokenAuthentication/JwtTokenGenerator.cs:
csharpusing Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TunnlR.Domain.Entities;

namespace TunnlR.Infrastructure.TokenAuthentication
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly IConfiguration _configuration;

        public JwtTokenGenerator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(AppUser user)
        {
            var securityKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
Extensions/ServiceCollectionExtensions.cs:
csharpusing Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TunnlR.Domain.Entities;
using TunnlR.Infrastructure.Persistence;
using TunnlR.Infrastructure.TokenAuthentication;

namespace TunnlR.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services, 
            IConfiguration configuration)
        {
            // Database
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string not found.");
            
            services.AddDbContext<TunnelDbContext>(options =>
                options.UseSqlite(connectionString));

            // Identity
            services.AddIdentity<AppUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<TunnelDbContext>()
                .AddDefaultTokenProviders();

            // JWT Token Generator
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}
```

---

## **3. TunnlR.Application Layer**

### Purpose: Business logic and service interfaces

### Folders & Files:
```
TunnlR.Application/
├── Services/
│   ├── Auth/
│   │   ├── IAuthenticationService.cs
│   │   └── AuthenticationService.cs
│   │
│   ├── Tunnel/
│   │   ├── ITunnelService.cs
│   │   └── TunnelService.cs
│   │
│   └── WebSocket/
│       ├── IWebSocketConnectionManager.cs
│       └── WebSocketConnectionManager.cs
│
└── Extensions/
    └── ServiceCollectionExtensions.cs
What to do:
Services/Auth/IAuthenticationService.cs:
csharpusing TunnlR.Domain.DTOs.Auth;

namespace TunnlR.Application.Services.Auth
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
}
Services/Auth/AuthenticationService.cs:
csharpusing Microsoft.AspNetCore.Identity;
using TunnlR.Application.Services.Auth;
using TunnlR.Domain.DTOs.Auth;
using TunnlR.Domain.Entities;
using TunnlR.Infrastructure.TokenAuthentication;

namespace TunnlR.Application.Services.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenGenerator _tokenGenerator;

        public AuthenticationService(
            UserManager<AppUser> userManager,
            IJwtTokenGenerator tokenGenerator)
        {
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Invalid credentials");

            var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isValid)
                throw new UnauthorizedAccessException("Invalid credentials");

            var token = _tokenGenerator.GenerateToken(user);

            return new LoginResponse
            {
                Token = token,
                Email = user.Email!,
                ExpiresAt = DateTime.UtcNow.AddHours(24)
            };
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            if (request.Password != request.ConfirmPassword)
                return new RegisterResponse 
                { 
                    Success = false, 
                    Message = "Passwords do not match" 
                };

            var user = new AppUser
            {
                Email = request.Email,
                UserName = request.Email,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return new RegisterResponse
                {
                    Success = false,
                    Message = string.Join(", ", result.Errors.Select(e => e.Description))
                };

            return new RegisterResponse
            {
                Success = true,
                Message = "Registration successful"
            };
        }
    }
}
Services/Tunnel/ITunnelService.cs:
csharpusing TunnlR.Domain.DTOs.Tunnel;

namespace TunnlR.Application.Services.Tunnel
{
    public interface ITunnelService
    {
        Task<TunnelCreateResponse> CreateTunnelAsync(Guid userId, TunnelCreateRequest request);
        Task<TunnelStatusResponse> GetTunnelStatusAsync(Guid tunnelId);
        Task DeactivateTunnelAsync(Guid tunnelId);
    }
}
Services/Tunnel/TunnelService.cs:
csharpusing Microsoft.EntityFrameworkCore;
using TunnlR.Application.Services.Tunnel;
using TunnlR.Domain.DTOs.Tunnel;
using TunnlR.Infrastructure.Persistence;

namespace TunnlR.Application.Services.Tunnel
{
    public class TunnelService : ITunnelService
    {
        private readonly TunnelDbContext _context;

        public TunnelService(TunnelDbContext context)
        {
            _context = context;
        }

        public async Task<TunnelCreateResponse> CreateTunnelAsync(
            Guid userId, 
            TunnelCreateRequest request)
        {
            // Generate unique subdomain
            var subdomain = Guid.NewGuid().ToString("N").Substring(0, 8);
            var publicUrl = $"https://{subdomain}.tunnlr.dev";
            var dashboardUrl = $"https://dashboard.tunnlr.dev/{subdomain}";

            var tunnel = new Domain.Entities.Tunnel
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PublicUrl = publicUrl,
                DashboardUrl = dashboardUrl,
                LocalPort = request.LocalPort,
                Protocol = request.Protocol,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Tunnels.Add(tunnel);
            await _context.SaveChangesAsync();

            return new TunnelCreateResponse
            {
                TunnelId = tunnel.Id,
                PublicUrl = tunnel.PublicUrl,
                DashboardUrl = tunnel.DashboardUrl
            };
        }

        public async Task<TunnelStatusResponse> GetTunnelStatusAsync(Guid tunnelId)
        {
            var tunnel = await _context.Tunnels.FindAsync(tunnelId);
            if (tunnel == null)
                throw new KeyNotFoundException("Tunnel not found");

            var sessions = await _context.TunnelSessions
                .Where(s => s.TunnelId == tunnelId)
                .ToListAsync();

            return new TunnelStatusResponse
            {
                TunnelId = tunnel.Id,
                IsActive = tunnel.IsActive,
                BytesTransferred = sessions.Sum(s => s.BytesTransferred),
                RequestCount = sessions.Count
            };
        }

        public async Task DeactivateTunnelAsync(Guid tunnelId)
        {
            var tunnel = await _context.Tunnels.FindAsync(tunnelId);
            if (tunnel != null)
            {
                tunnel.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}
Services/WebSocket/IWebSocketConnectionManager.cs:
csharpusing System.Net.WebSockets;

namespace TunnlR.Application.Services.WebSocket
{
    public interface IWebSocketConnectionManager
    {
        void AddConnection(string connectionId, System.Net.WebSockets.WebSocket socket);
        System.Net.WebSockets.WebSocket? GetConnection(string connectionId);
        void RemoveConnection(string connectionId);
        Task SendMessageAsync(string connectionId, string message);
        Task BroadcastAsync(string message);
    }
}
Services/WebSocket/WebSocketConnectionManager.cs:
csharpusing System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace TunnlR.Application.Services.WebSocket
{
    public class WebSocketConnectionManager : IWebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> _connections = new();

        public void AddConnection(string connectionId, System.Net.WebSockets.WebSocket socket)
        {
            _connections.TryAdd(connectionId, socket);
        }

        public System.Net.WebSockets.WebSocket? GetConnection(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var socket);
            return socket;
        }

        public void RemoveConnection(string connectionId)
        {
            _connections.TryRemove(connectionId, out _);
        }

        public async Task SendMessageAsync(string connectionId, string message)
        {
            var socket = GetConnection(connectionId);
            if (socket?.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }

        public async Task BroadcastAsync(string message)
        {
            var tasks = _connections.Values
                .Where(s => s.State == WebSocketState.Open)
                .Select(async socket =>
                {
                    var bytes = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                });

            await Task.WhenAll(tasks);
        }
    }
}
Extensions/ServiceCollectionExtensions.cs:
csharpusing Microsoft.Extensions.DependencyInjection;
using TunnlR.Application.Services.Auth;
using TunnlR.Application.Services.Tunnel;
using TunnlR.Application.Services.WebSocket;

namespace TunnlR.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITunnelService, TunnelService>();
            services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();

            return services;
        }
    }
}
```

---

## **4. TunnlR.API Layer**

### Purpose: HTTP endpoints and WebSocket handling

### Folders & Files:
```
TunnlR.API/
├── Controllers/
│   ├── AuthController.cs
│   └── TunnelController.cs
│
├── WebSockets/
│   └── TunnelWebSocketHandler.cs
│
├── Program.cs
└── appsettings.json
What to do:
Controllers/AuthController.cs:
csharpusing Microsoft.AspNetCore.Mvc;
using TunnlR.Application.Services.Auth;
using TunnlR.Domain.DTOs.Auth;

namespace TunnlR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                var response = await _authService.LoginAsync(request);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var response = await _authService.RegisterAsync(request);
            
            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }
    }
}
Controllers/TunnelController.cs:
csharpusing Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TunnlR.Application.Services.Tunnel;
using TunnlR.Domain.DTOs.Tunnel;

namespace TunnlR.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TunnelController : ControllerBase
    {
        private readonly ITunnelService _tunnelService;

        public TunnelController(ITunnelService tunnelService)
        {
            _tunnelService = tunnelService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTunnel([FromBody] TunnelCreateRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _tunnelService.CreateTunnelAsync(userId, request);
            return Ok(response);
        }

        [HttpGet("{tunnelId}/status")]
        public async Task<IActionResult> GetStatus(Guid tunnelId)
        {
            var response = await _tunnelService.GetTunnelStatusAsync(tunnelId);
            return Ok(response);
        }

        [HttpDelete("{tunnelId}")]
        public async Task<IActionResult> Deactivate(Guid tunnelId)
        {
            await _tunnelService.DeactivateTunnelAsync(tunnelId);
            return NoContent();
        }
    }
}
WebSockets/TunnelWebSocketHandler.cs:
csharpusing Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TunnlR.Application.Services.Tunnel;
using TunnlR.Application.Services.WebSocket;
using TunnlR.Domain.DTOs.Tunnel;

namespace TunnlR.API.WebSockets
{
    public class TunnelWebSocketHandler
    {
        private readonly IWebSocketConnectionManager _connectionManager;
        private readonly ITunnelService _tunnelService;

        public TunnelWebSocketHandler(
            IWebSocketConnectionManager connectionManager,
            ITunnelService tunnelService)
        {
            _connectionManager = connectionManager;
            _tunnelService = tunnelService;
        }

        public async Task HandleConnectionAsync(
            HttpContext context, 
            System.Net.WebSockets.WebSocket webSocket)
        {
            var connectionId = Guid.NewGuid().ToString();
            _connectionManager.AddConnection(connectionId, webSocket);

            try
            {
                // Extract user ID from token
                var token = context.Request.Query["token"].ToString();
                var userId = ExtractUserIdFromToken(token);

                // Extract port from query
                var port = int.Parse(context.Request.Query["port"].ToString());
                var protocol = context.Request.Query["protocol"].ToString();

                // Create tunnel
                var tunnelResponse = await _tunnelService.CreateTunnelAsync(userId, new TunnelCreateRequest
                {
                    LocalPort = port,
                    Protocol = protocol
                });

                // Send tunnel info to CLI
                var message = JsonSerializer.Serialize(tunnelResponse);
                await _connectionManager.SendMessageAsync(connectionId, message);

                // Keep connection alive and handle messages
                await ReceiveMessagesAsync(webSocket, connectionId);
            }
            finally
            {
                _connectionManager.RemoveConnection(connectionId);
            }
        }

        private async Task ReceiveMessagesAsync(System.Net.WebSockets.WebSocket webSocket, string connectionId)
        {
            var buffer = new byte[1024 * 4];

            while (webSocket.State == WebSocketState.Open)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);
                }
                else
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    
                    // Handle incoming traffic from CLI
                    // Forward to appropriate destination
                    Console.WriteLine($"Received from {connectionId}: {message}");
                }
            }
        }

        private Guid ExtractUserIdFromToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var userIdClaim = jwtToken.Claims.First(c => c.Type == "nameid").Value;
            return Guid.Parse(userIdClaim);
        }
    }
}
Program.cs:
csharpusing Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TunnlR.Application.Extensions;
using TunnlR.Infrastructure.Extensions;
using TunnlR.API.WebSockets;
using TunnlR.Application.Services.WebSocket;
using TunnlR.Application.Services.Tunnel;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add layers
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();

// WebSocket endpoint
app.Map("/tunnel", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var handler = context.RequestServices.GetRequiredService<TunnelWebSocketHandler>();
        await handler.HandleConnectionAsync(context, webSocket);
    }
    else
    {
        context.Response.StatusCode = 400;
    }
});

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
appsettings.json:
json{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tunnlr.db"
  },
  "Jwt": {
    "Key": "your-super-secret-key-min-32-characters-long",
    "Issuer": "TunnlR",
    "Audience": "TunnlR-CLI"
  }
}
```

---

## **5. TunnlR.CLI Layer**

### Purpose: Command-line interface for users

### Folders & Files:
```
TunnlR.CLI/
├── Commands/
│   ├── LoginCommand.cs
│   ├── RegisterCommand.cs
│   ├── StartCommand.cs
│   ├── StatusCommand.cs
│   └── StopCommand.cs
│
├── Services/
│   ├── AuthenticationService.cs
│   ├── TunnelService.cs
│   └── TokenStorage.cs
│
├── Helpers/
│   └── ConsoleHelpers.cs
│
├── Program.cs
└── appsettings.json
What to do:
Services/TokenStorage.cs:
csharpnamespace TunnlRCLI.Services
{
    public class TokenStorage
    {
        private readonly string _tokenFilePath;

        public TokenStorage()
        {
            var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var tunnlrDir = Path.Combine(appDataPath, ".tunnlr");
            Directory.CreateDirectory(tunnlrDir);
            _tokenFilePath = Path.Combine(tunnlrDir, "token.txt");
        }

        public async Task SaveTokenAsync(string token)
        {
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
Services/AuthenticationService.cs:
csharpusing System.Net.Http.Json;
using TunnlR.Domain.DTOs.Auth;

namespace TunnlRCLI.Services
{
    public class AuthenticationService
    {
        private readonly HttpClient _httpClient;

        public AuthenticationService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            var serverUrl = configuration["RelayServer:HttpUrl"]!;
            _httpClient.BaseAddress = new Uri(serverUrl);
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var request = new LoginRequest { Email = email, Password = password };
            
            var response = await _httpClient.PostAsJsonAsync("/api/auth/login", request);
            response.EnsureSuccessStatusCode();
            
            return await response.Content.ReadFromJsonAsync<LoginResponse>()
                ?? throw new Exception("Failed to deserialize response");
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
            
            return await response.Content.ReadFromJsonAsync<RegisterResponse>()
                ?? throw new Exception("Failed to deserialize response");
        }
    }
}
Services/TunnelService.cs:
csharpusing System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TunnlR.Domain.DTOs.Tunnel;
using TunnlR.Domain.Entities;

namespace TunnlRCLI.Services
{
    public class TunnelService
    {
        private readonly IConfiguration _configuration;
        private ClientWebSocket? _webSocket;
        private Tunnel? _currentTunnel;

        public event EventHandler<string>? MessageReceived;
        public event EventHandler<TunnelCreateResponse>? TunnelEstablished;
        public event EventHandler? TunnelClosed;

        public TunnelService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task ConnectAsync(string token, int localPort, string protocol)
        {
            var wsUrl = _configuration["RelayServer:WebSocketUrl"]!;
            _webSocket = new ClientWebSocket();

            var uri = new Uri($"{wsUrl}/tunnel?token={token}&port={localPort}&protocol={protocol}");
            
            await _webSocket.ConnectAsync(uri, CancellationToken.None);

            Console.WriteLine("✅ Connected to relay server!");

            _ = Task.Run(ListenAsync);
        }

        private async Task ListenAsync()
        {
            var buffer = new byte[1024 * 4];

            while (_webSocket?.State == WebSocketState.Open)
            {
                var result = await _webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await _webSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);
                    
                    TunnelClosed?.Invoke(this, EventArgs.Empty);
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    await HandleMessageAsync(message);
                }
            }
        }

        private async Task HandleMessageAsync(string message)
        {
            try
            {
                var tunnelInfo = JsonSerializer.Deserialize<TunnelCreateResponse>(message);
                
                if (tunnelInfo != null)
                {
                    TunnelEstablished?.Invoke(this, tunnelInfo);
                }
                else
                {
                    MessageReceived?.Invoke(this, message);
                }
            }
            catch
            {
                MessageReceived?.Invoke(this, message);
            }
        }

        public async Task SendAsync(string message)
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                await _webSocket.SendAsync(
                    new ArraySegment<byte>(bytes),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
        }

        public async Task DisconnectAsync()
        {
            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(
                    WebSocketCloseStatus.NormalClosure,
                    "User disconnected",
                    CancellationToken.None);
            }
        }
    }
}
Commands/LoginCommand.cs:
csharpusing TunnlRCLI.Services;

namespace TunnlRCLI.Commands
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
                Console.WriteLine("Logging in...");
                
                var response = await _authService.LoginAsync(email, password);
                await _tokenStorage.SaveTokenAsync(response.Token);
                
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ Login successful!");
                Console.WriteLine($"Email: {response.Email}");
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
Commands/RegisterCommand.cs:
csharpusing TunnlRCLI.Services;

namespace TunnlRCLI.Commands
{
    public class RegisterCommand
    {
        private readonly AuthenticationService _authService;

        public RegisterCommand(AuthenticationService authService)
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
Commands/StartCommand.cs:
csharpusing TunnlR.Domain.DTOs.Tunnel;
using TunnlRCLI.Helpers;
using TunnlRCLI.Services;

namespace TunnlRCLI.Commands
{
    public class StartCommand
    {
        private readonly TunnelService _tunnelService;
        private readonly TokenStorage _tokenStorage;

        public StartCommand(TunnelService tunnelService, TokenStorage tokenStorage)
        {
            _tunnelService = tunnelService;
            _tokenStorage = tokenStorage;
        }

        public async Task ExecuteAsync(int port, string protocol = "http")
        {
            var token = await _tokenStorage.GetTokenAsync();
            
            if (string.IsNullOrEmpty(token))
            {
                Console.WriteLine("❌ Not logged in. Run 'tunnlr login' first.");
                return;
            }

            Console.WriteLine($"Starting tunnel on port {port}...");
            ConsoleHelpers.PrintLoadingBar("Connecting", 20, 50);
            
            // Subscribe to tunnel established event
            _tunnelService.TunnelEstablished += (sender, tunnelInfo) =>
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("\n✅ Tunnel started!");
                Console.WriteLine($"Public URL: {tunnelInfo.PublicUrl}");
                Console.WriteLine($"Dashboard: {tunnelInfo.DashboardUrl}");
                Console.ResetColor();
            };

            await _tunnelService.ConnectAsync(token, port, protocol);
            
            Console.WriteLine("\nPress Ctrl+C to stop the tunnel...");
            await Task.Delay(Timeout.Infinite);
        }
    }
}
Program.cs:
csharpusing Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TunnlRCLI.Commands;
using TunnlRCLI.Helpers;
using TunnlRCLI.Services;

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

builder.Services.AddHttpClient<AuthenticationService>();
builder.Services.AddSingleton<TunnelService>();
builder.Services.AddSingleton<TokenStorage>();
builder.Services.AddSingleton<LoginCommand>();
builder.Services.AddSingleton<RegisterCommand>();
builder.Services.AddSingleton<StartCommand>();

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
                var registerCmd = host.Services.GetRequiredService<RegisterCommand>();
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
appsettings.json:
json{
  "RelayServer": {
    "HttpUrl": "https://your-ec2-domain.com",
    "WebSocketUrl": "wss://your-ec2-domain.com"
  }
}
