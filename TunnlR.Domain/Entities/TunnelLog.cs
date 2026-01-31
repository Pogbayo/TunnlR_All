using Application.Enums;

namespace Application.Entities
{
    public class TunnelLog
    {
        public Guid Id { get; set; }                     
        public Guid TunnelId { get; set; }               
        public DateTime Timestamp { get; set; }
        public TunnelEventTypeLog EventType { get; set; }  
        public string Message { get; set; } = string.Empty;
    }
}
