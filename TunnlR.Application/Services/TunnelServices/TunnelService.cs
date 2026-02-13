using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Application.Mappings;
using TunnlR.Application.Services.WebSocketConnection;
using TunnlR.Contract.DTOs.Enums;
using TunnlR.Contract.DTOs.TunnelDto;
using TunnlR.Domain.Entities;
using TunnlR.RelayServer.Persistence;

namespace TunnlR.Application.Services.TunnelServices
{
    public class TunnelService : ITunnelService
    {
        private readonly TunnelDbContext _context;
        private readonly IWebSocketConnectionManager _wsManager;
        private readonly IConfiguration _configuration;
        public TunnelService(IConfiguration configuration,TunnelDbContext context, IWebSocketConnectionManager wsManager)
        {
            _configuration = configuration;
            _context = context;
            _wsManager = wsManager;
        }

        public async Task<TunnelCreateResponse> CreateTunnelAsync(
        Guid userId,
        TunnelCreateRequest request)
        {
            Console.WriteLine($"CreateTunnelAsync called for userId: {userId}, LocalPort: {request.LocalPort}, Protocol: {request.Protocol}");

            var existingTunnel = await _context.Tunnels
                .FirstOrDefaultAsync(t => t.UserId == userId && t.Status == Domain.DTOs.Enums.TunnelStatus.Active);

            Console.WriteLine(existingTunnel != null
                ? $"Found existing ACTIVE tunnel: {existingTunnel.Id}"
                : "No active tunnel found for user");

            if (existingTunnel != null)
            {
                Console.WriteLine("Reusing existing tunnel and setting to Active");
                existingTunnel.Status = Domain.DTOs.Enums.TunnelStatus.Active;
                existingTunnel.LocalPort = request.LocalPort;
                await _context.SaveChangesAsync();
                Console.WriteLine($"Reused tunnel saved. Returning TunnelId: {existingTunnel.Id}");

                return new TunnelCreateResponse
                {
                    TunnelId = existingTunnel.Id,
                    PublicUrl = existingTunnel.PublicUrl,
                    DashboardUrl = existingTunnel.DashboardUrl
                };
            }
            else
            {
                Console.WriteLine("Creating new tunnel");
                var host = _configuration.GetValue<string>("ServerUrl:Host");
                var subdomain = Guid.NewGuid().ToString("N").Substring(0, 8);
                var publicUrl = $"https://{subdomain}.{host}";
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
                    Status = Domain.DTOs.Enums.TunnelStatus.Active
                };

                Console.WriteLine($"New tunnel object created with Id: {tunnel.Id}, Subdomain: {subdomain}");

                _context.Tunnels.Add(tunnel);
                await _context.SaveChangesAsync();

                Console.WriteLine($"New tunnel saved to DB. TunnelId: {tunnel.Id}");

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


        public async Task<Tunnel?> GetTunnelById(Guid id)
        {
            var tunnel =  await _context.Tunnels.FindAsync(id);
            if (tunnel == null)
            {
                Console.WriteLine("Tunnel is null");
                throw new Exception($"Tunnel with Id '{id}' not found");
            }
            return tunnel;
        }

        public async Task DeactivateTunnelAsync(Guid tunnelId)
        {
            var tunnel = await _context.Tunnels.FindAsync(tunnelId);
            if (tunnel != null)
            {
                tunnel.Status = (Domain.DTOs.Enums.TunnelStatus)TunnelStatus.Inactive;
                await _context.SaveChangesAsync();
            }

            var controlMessage = JsonSerializer.Serialize(new
            {
                Type = "TUNNEL_CLOSED",
                Reason = "Tunnel deactivated by server"
            });

            await _wsManager.SendMessageAsync(tunnelId, controlMessage);

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
                throw new Exception($"Tunnel with subdomain '{subdomain}' not found");
            }

            return tunnel!;
        }

    }
}