using TunnlR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TunnlR.Infrastructure.Configurations
{
    public class TunnelTrafficConfiguration : IEntityTypeConfiguration<TunnelTraffic>
    {
        public void Configure(EntityTypeBuilder<TunnelTraffic> builder)
        {

            builder.HasKey(t => t.Id);

            builder.Property(t => t.ClientIp)
                .HasMaxLength(45);

            builder.HasIndex(t => t.TunnelId);
        }
    }
}