
using TunnlR.Contract.DTOs.Auth;

namespace TunnlR.Application.Interfaces.IService
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
    }
}
