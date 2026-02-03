using Microsoft.EntityFrameworkCore;
using TunnlR.Application.DTOs.Tunnel;
using TunnlR.Application.Interfaces.IService;
using TunnlR.RelayServer.Persistence;
using Domain.Enums;
using TunnlR.Domain.Entities;

namespace TunnlR.Application.Services.TunnelServices
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
            string connectionId,
            TunnelCreateRequest request)
        {
            // Check if user already has an active tunnel
            var existingTunnel = await _context.Tunnels
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Status == TunnelStatus.Active);

            if (existingTunnel != null)
            {
                existingTunnel.ConnectionId = connectionId;
                existingTunnel.Status = TunnelStatus.Active;
                existingTunnel.LocalPort = request.LocalPort;
                await _context.SaveChangesAsync();

                return new TunnelCreateResponse
                {
                    TunnelId = existingTunnel.Id,
                    PublicUrl = existingTunnel.PublicUrl,
                    DashboardUrl = existingTunnel.DashboardUrl
                };
            }
            else
            {
                var subdomain = Guid.NewGuid().ToString("N").Substring(0, 8);
                var publicUrl = $"https://{subdomain}.tunnlr.dev";
                var dashboardUrl = $"https://dashboard.tunnlr.dev/{subdomain}";

                var tunnel = new Tunnel
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    ConnectionId = connectionId,
                    PublicUrl = publicUrl,
                    DashboardUrl = dashboardUrl,
                    LocalPort = request.LocalPort,
                    Protocol = request.Protocol,
                    CreatedAt = DateTime.UtcNow,
                    Status = TunnelStatus.Active
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
        }

        public async Task<TunnelStatusResponse> GetTunnelStatusAsync(Guid tunnelId)
        {
            var tunnel = await _context.Tunnels.FindAsync(tunnelId);
            if (tunnel == null)
                throw new KeyNotFoundException("Tunnel not found");

            var sessions = await _context.Tunnels
                .Include(s => s.TunnelTraffics)
                .Where(s => s.Id == tunnelId)
                .ToListAsync();

            return new TunnelStatusResponse
            {
                TunnelId = tunnel.Id,
                TunnelStatus = TunnelStatus.Active,
                //BytesTransferred = sessions.TunnelTraffics.Sum(s => s.BytesTransferred),
                RequestCount = sessions.Count
            };
        }

        public async Task DeactivateTunnelAsync(Guid tunnelId)
        {
            var tunnel = await _context.Tunnels.FindAsync(tunnelId);
            if (tunnel != null)
            {
                tunnel.Status = TunnelStatus.Inactive;
                await _context.SaveChangesAsync();
            }
        }
    }
}