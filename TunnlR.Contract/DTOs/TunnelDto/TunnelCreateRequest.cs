namespace TunnlR.Contract.DTOs.TunnelDto
{
    public class TunnelCreateRequest
    {
        public Guid TunnelId { get; set; }
        public int LocalPort { get; set; }
        public string Protocol { get; set; } = "http";
    }
}
