namespace TunnlR.Application.DTOs.Tunnel
{
    public class TunnelCreateResponse
    {
        public Guid TunnelId { get; set; }
        public string PublicUrl { get; set; } = string.Empty;
        public string DashboardUrl { get; set; } = string.Empty;
    }
}
