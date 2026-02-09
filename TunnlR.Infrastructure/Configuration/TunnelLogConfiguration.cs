using TunnlR.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TunnlR.Infrastructure.Configurations
{
    public class TunnelLogConfiguration : IEntityTypeConfiguration<TunnelLog>
    {
        public void Configure(EntityTypeBuilder<TunnelLog> builder)
        {
            builder.Property(l => l.Message)
                .HasMaxLength(1000);

            builder.Property(l => l.EventType)
                .HasConversion<string>();  

            builder.HasIndex(l => l.TunnelId);
        }
    }
}