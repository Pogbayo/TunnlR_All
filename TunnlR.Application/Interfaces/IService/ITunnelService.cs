
using TunnlR.Application.DTOs.Tunnel;
using TunnlR.Application.DTOs.TunnelDto;

namespace TunnlR.Application.Interfaces.IService
{
    public interface ITunnelService
    {
        Task<TunnelCreateResponse> CreateTunnelAsync(Guid userId, TunnelCreateRequest request);
        Task<TunnelStatusResponse> GetTunnelStatusAsync(Guid tunnelId);
        Task DeactivateTunnelAsync(Guid tunnelId);
        Task<GetTunnelResponse> GetTunnelBySubDomain(string subdomain);
    }
}
