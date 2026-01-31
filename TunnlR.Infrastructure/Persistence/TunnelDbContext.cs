using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using TunnlR.Domain;

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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<BaseEntity>()
                 .HasQueryFilter(e => !e.IsDeleted);

            builder.ApplyConfigurationsFromAssembly(typeof(TunnelDbContext).Assembly);
        }
    }
}
