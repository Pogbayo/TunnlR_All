using TunnlR.Contract.DTOs.Enums;
namespace TunnlR.Contract.DTOs.TunnelDto
{
    public class GetTunnelResponse
    {
        public Guid TunnelId { get; set; }
        public string PublicUrl { get; set; } = string.Empty;
        public TunnelStatus Status { get; set; }
        //public string DashboardUrl { get; set; } = string.Empty;
    }
}
