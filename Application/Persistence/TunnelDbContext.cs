
using Application.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Persistence
{
    public class TunnelDbContext : DbContext
    {
        public TunnelDbContext(DbContextOptions<TunnelDbContext> options) : base(options) { }
        public DbSet<Tunnel> Tunnels { get; set; } 
        public DbSet<TunnelLog> TunnelLogs { get; set; }
        public DbSet<TunnelConnection> TunnelConnections { get; set; }
    }
}
