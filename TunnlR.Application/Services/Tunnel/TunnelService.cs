using Microsoft.EntityFrameworkCore;
using TunnlR.Application.DTOs.Tunnel;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Application.Services.Tunnel;
using TunnlR.Domain.DTOs.Tunnel;
using TunnlR.Infrastructure.Persistence;
using TunnlR.RelayServer.Persistence;

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

            var sessions = await _context.Tunnel
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