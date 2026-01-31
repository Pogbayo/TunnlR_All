

namespace Application.Entities
{
    public class TunnelLog
    {
        public int Id { get; set; }                     // PK, auto-increment
        public int TunnelId { get; set; }               // FK
        public DateTime Timestamp { get; set; }
        public string EventType { get; set; } = string.Empty;  // "Started", "Stopped", "Connected", etc.
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }            // nullable, for extra info or JSON
    }
}
