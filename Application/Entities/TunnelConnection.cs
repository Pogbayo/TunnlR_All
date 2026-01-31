namespace Application.Entities
{
    public class TunnelConnection
    {
        public int Id { get; set; }                    
        public int TunnelId { get; set; }            
        public string ClientIp { get; set; } = string.Empty;
        public DateTime ConnectedAt { get; set; }
        public DateTime? DisconnectedAt { get; set; }   
        public long BytesSent { get; set; }
        public long BytesReceived { get; set; }
    }
}
