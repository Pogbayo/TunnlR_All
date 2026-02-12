using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using TunnlR.Contract.DTOs.TunnelDto;
using TunnlR.Domain.Entities;

namespace TunnlR.Application.Interfaces.IService
{
    public interface ITunnelUrlHandler
    {
        bool TryGetTunnelId(PathString path, out Guid tunnelId);
        string GetLocalPath(PathString path);
        Task<GetTunnelResponse?> GetTunnelForSubdomainAsync(string host);
        Task<Tunnel?> GetTunnelForIdAsync(Guid tunnelId);
        bool IsSubdomainValid(string host);
        Task<WebSocket?> GetActiveSocketAsync(Guid tunnelId);
    }
}
