
using TunnlR.Application.DTOs.Tunnel;

namespace TunnlR.Application.Interfaces.IService
{
    public interface ITunnelService
    {
        Task<TunnelCreateResponse> CreateTunnelAsync(Guid userId,string connectionId, TunnelCreateRequest request);
        Task<TunnelStatusResponse> GetTunnelStatusAsync(Guid tunnelId);
        Task DeactivateTunnelAsync(Guid tunnelId);
    }
}
