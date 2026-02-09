using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Contract.DTOs.Auth;
using TunnlR.Domain.Entities;
using TunnlR.Infrastructure.TokenAuthentication;

namespace TunnlR.Application.Services.Auth
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtTokenGenerator _tokenGenerator;
        private readonly ILogger<AuthenticationService> _logger;

        public AuthenticationService(
            UserManager<AppUser> userManager,
            IJwtTokenGenerator tokenGenerator,
            ILogger<AuthenticationService> logger)
        {
            _userManager = userManager;
            _tokenGenerator = tokenGenerator;
            _logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning("Login failed: user not found - {Email}", request.Email);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                var isValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!isValid)
                {
                    _logger.LogWarning("Login failed: incorrect password for {Email}", request.Email);
                    throw new UnauthorizedAccessException("Invalid credentials");
                }

                var token = _tokenGenerator.GenerateToken(user);

                return new LoginResponse
                {
                    Token = token,
                    ExpiresAt = DateTime.UtcNow.AddHours(24)
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during login for {Email}", request.Email);
                throw new Exception("Login failed due to internal error. Please check the logs.");
            }
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
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
                    CreatedAt = DateTime.UtcNow,
                    LastLoginAt = DateTime.UtcNow

                };

                var result = await _userManager.CreateAsync(user, request.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning("Registration failed for {Email}: {Errors}", request.Email, errors);

                    return new RegisterResponse
                    {
                        Success = false,
                        Message = errors
                    };
                }

                return new RegisterResponse
                {
                    Success = true,
                    Message = "Registration successful"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during registration for {Email}", request.Email);
                return new RegisterResponse
                {
                    Success = false,
                    Message = "Registration failed due to internal error. Please check the logs."
                };
            }
        }
    }
}
