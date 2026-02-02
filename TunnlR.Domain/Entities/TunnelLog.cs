using Application.Enums;

namespace TunnlR.Domain.Entities
{
    public class TunnelLog : BaseEntity
    {
        public Guid TunnelId { get; set; }               
        public DateTime Timestamp { get; set; }
        public TunnelEventTypeLog EventType { get; set; }  
        public string Message { get; set; } = string.Empty;
    }
}
