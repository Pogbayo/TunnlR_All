using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TunnlR.Domain.Entities;

namespace TunnlR.Infrastructure.Configurations
{
    public class TunnelConfiguration : IEntityTypeConfiguration<Tunnel>
    {
        public void Configure(EntityTypeBuilder<Tunnel> builder)
        {
            builder.Property(t => t.PublicUrl)
                .IsRequired()
                .HasMaxLength(70);

            builder.Property(t => t.DashboardUrl)
                .IsRequired()
                .HasMaxLength(70);

            builder.Property(t => t.Protocol)
                .HasMaxLength(5);

            //builder.Property(t => t.Status)
            //    .HasConversion<string>(); 

            builder.HasMany(t => t.TunnelTraffics)
                .WithOne()  
                .HasForeignKey(c => c.TunnelId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.TunnelLogs)
                .WithOne()
                .HasForeignKey(l => l.TunnelId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}