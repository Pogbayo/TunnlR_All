using TunnlR.Contract.DTOs.Auth;
using TunnlR.Contract.DTOs.TunnelDto;
using TunnlR.Domain.Entities;

namespace TunnlR.Application.Interfaces.IService
{
    public interface ITunnelService
    {
        Task<TunnelCreateResponse> CreateTunnelAsync(Guid userId, TunnelCreateRequest request);
        Task<TunnelStatusResponse> GetTunnelStatusAsync(Guid tunnelId);
        Task DeactivateTunnelAsync(Guid tunnelId);
        Task<Tunnel?> GetTunnelById(Guid id);
        Task<GetTunnelResponse> GetTunnelBySubDomain(string subdomain);
    }
}
