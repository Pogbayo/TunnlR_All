using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TunnlR.Domain.Entities
{
    public class AppUser : IdentityUser<Guid>
    {

        [Required]
        [EmailAddress]
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public ICollection<Tunnel> Tunnels { get; set; } = new List<Tunnel>();

        public void MarkAsDeleted() => IsDeleted = true;

    }
}