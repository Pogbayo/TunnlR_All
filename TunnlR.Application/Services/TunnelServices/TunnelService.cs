using Microsoft.EntityFrameworkCore;
using TunnlR.Application.Interfaces.IService;
using TunnlR.RelayServer.Persistence;
using TunnlR.Domain.Entities;
using TunnlR.Contract.DTOs.Enums;
using TunnlR.Application.Mappings;
using TunnlR.Contract.DTOs.TunnelDto;

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
            TunnelCreateRequest request)
        {
            var existingTunnel = await _context.Tunnels
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Status == Domain.DTOs.Enums.TunnelStatus.Inactive);

            if (existingTunnel != null)
            {
                existingTunnel.Status = Domain.DTOs.Enums.TunnelStatus.Inactive;
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
                    PublicUrl = publicUrl,
                    DashboardUrl = dashboardUrl,
                    LocalPort = request.LocalPort,
                    Protocol = request.Protocol,
                    CreatedAt = DateTime.UtcNow,
                    Status = Domain.DTOs.Enums.TunnelStatus.Inactive 
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
                tunnel.Status = (Domain.DTOs.Enums.TunnelStatus)TunnelStatus.Inactive;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<GetTunnelResponse> GetTunnelBySubDomain(string subdomain)
        {
            var tunnel = await _context.Tunnels
                 .Where(t => t.PublicUrl.Contains(subdomain))
                 .Select(t => new GetTunnelResponse
                 {
                     TunnelId = t.Id,
                     PublicUrl = t.PublicUrl,
                     Status = TunnelStatusMapper.ToContract(t.Status)
                 })
                .FirstOrDefaultAsync();
            if (tunnel == null)
            {
                Console.WriteLine("Tunnel is null");
            }

            return tunnel!;
        }

    }
}