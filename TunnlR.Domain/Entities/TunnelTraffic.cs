namespace TunnlR.Domain.Entities
{
    public class TunnelTraffic : BaseEntity
    {
        public Guid TunnelId { get; set; }            
        public string ClientIp { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime? DisconnectedAt { get; set; }   
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
    }
}
