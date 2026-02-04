using Domain.Enums;

namespace TunnlR.Domain.Entities
{
    public class Tunnel : BaseEntity
    {
        public int LocalPort { get; set; }
        public required string PublicUrl { get; set; }
        public required string DashboardUrl { get; set; }
        public string Protocol { get; set; } = "https";  
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }          
        public TunnelStatus Status { get; set; }  

        public Guid UserId { get; set; }               
        public AppUser? User { get; set; }

        public ICollection<TunnelTraffic> TunnelTraffics { get; set; } = new List<TunnelTraffic>();
        public ICollection<TunnelLog> TunnelLogs { get; set; } = new List<TunnelLog>();
    }
}
