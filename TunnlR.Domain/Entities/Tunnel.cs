using Application.Enums;

namespace Application.Entities
{
    public class Tunnel
    {
        public Guid Id { get; set; }                     
        public int LocalPort { get; set; }
        public required string PublicUrl { get; set; }
        public required string DashboardUrl { get; set; }
        public string Protocol { get; set; } = "https";  
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }          
        public TunnelStatus Status { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }               
        public AppUser? User { get; set; }

        public ICollection<TunnelTraffic> Traffics { get; set; } = new List<TunnelTraffic>();
        public ICollection<TunnelLog> Logs { get; set; } = new List<TunnelLog>();
    }
}
