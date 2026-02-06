
using TunnlR.Contract.DTOs.Enums;
namespace TunnlR.Contract.DTOs.TunnelDto
{
    public class TunnelStatusResponse
    {
        public Guid TunnelId { get; set; }
        public TunnelStatus TunnelStatus { get; set; }
        public long? BytesTransferred { get; set; }
        public int RequestCount { get; set; }
    }
}
