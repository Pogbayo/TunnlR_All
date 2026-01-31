using System.ComponentModel.DataAnnotations;

namespace Application.Entities
{
    public class AppUser
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;  

        public string FullName { get; set; } = string.Empty;

        public string? AuthToken { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public ICollection<Tunnel> Tunnels { get; set; } = new List<Tunnel>();
    }
}