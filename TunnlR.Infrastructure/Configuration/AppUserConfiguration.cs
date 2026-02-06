using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TunnlR.Domain.Entities;

namespace TunnlR.Infrastructure.Configuration
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> builder)
        {

            builder.Property(u => u.CreatedAt)
                .ValueGeneratedOnAdd();

            builder.Property(u => u.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate();

            builder.HasQueryFilter(u => !u.IsDeleted);
            
            builder.HasMany(u => u.Tunnels)
                .WithOne(t => t.User)
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}
