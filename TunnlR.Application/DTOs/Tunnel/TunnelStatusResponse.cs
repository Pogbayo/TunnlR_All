namespace TunnlR.Application.DTOs.Tunnel
{
    public class TunnelStatusResponse
    {
        public Guid TunnelId { get; set; }
        public bool IsActive { get; set; }
        public long BytesTransferred { get; set; }
        public int RequestCount { get; set; }
    }
}
