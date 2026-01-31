using Domain.Entities;

namespace TunnlR.Infrastructure.TokenAuthentication
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(AppUser user);
    }
}
