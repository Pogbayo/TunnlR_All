using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using TunnlR.Domain;
using TunnlR.Domain.Entities;

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
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                {
                    var parameter = Expression.Parameter(entityType.ClrType, "e");
                    var property = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                    var filter = Expression.Lambda(Expression.Not(property), parameter);

                    builder.Entity(entityType.ClrType).HasQueryFilter(filter);
                }
            }

            builder.ApplyConfigurationsFromAssembly(typeof(TunnelDbContext).Assembly);
        }
    }
}
