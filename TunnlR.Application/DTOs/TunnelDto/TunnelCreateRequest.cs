
namespace TunnlR.Application.DTOs.Tunnel
{
    public class TunnelCreateRequest
    {
        public int LocalPort { get; set; }
        public string Protocol { get; set; } = "http";
    }
}
