namespace Tunnlr.Services
{

    public class Tunnel
    {
        public int LocalPort { get; set; }
        public required string PublicUrl { get; set; }
        public required string DashboardUrl { get; set; }
    }

    public class TunnelService
    {
        public Tunnel StartTunnel(int localPort, string protocol)
        {
            var tunnel = new Tunnel
            {
                LocalPort = localPort,
                PublicUrl = $"https://abc.dev.tunnels.ms/{localPort}",
                DashboardUrl = $"http://localhost:5001/dashboard/{localPort}"
            };

            Console.WriteLine($"[Event] TunnelStarted for port {localPort}");

            return tunnel;
        }
    }
}
