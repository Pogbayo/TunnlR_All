using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TunnlR.Infrastructure.Configurations
{
    public class TunnelConfiguration : IEntityTypeConfiguration<Tunnel>
    {
        public void Configure(EntityTypeBuilder<Tunnel> builder)
        {

            builder.HasKey(t => t.Id);

            builder.Property(t => t.PublicUrl)
                .IsRequired()
                .HasMaxLength(70);

            builder.Property(t => t.DashboardUrl)
                .IsRequired()
                .HasMaxLength(70);

            builder.Property(t => t.Protocol)
                .HasMaxLength(5);

            builder.Property(t => t.Status)
                .HasConversion<string>(); 

            builder.HasMany(t => t.Traffics)
                .WithOne()  
                .HasForeignKey(c => c.TunnelId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(t => t.Logs)
                .WithOne()
                .HasForeignKey(l => l.TunnelId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}