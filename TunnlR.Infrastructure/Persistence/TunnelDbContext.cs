using Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace TunnlR.RelayServer.Persistence
{
    public class TunnelDbContext : DbContext
    {
        public TunnelDbContext(DbContextOptions<TunnelDbContext> options)
            : base(options) { }

        public DbSet<Tunnel> Tunnels { get; set; }
        public DbSet<AppUser> Users { get; set; }
        public DbSet<TunnelLog> TunnelLogs { get; set; }
        public DbSet<TunnelTraffic> TunnelConnections { get; set; }
    }
}
