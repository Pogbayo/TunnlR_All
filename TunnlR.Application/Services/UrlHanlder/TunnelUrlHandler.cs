using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Contract.DTOs.TunnelDto;
using TunnlR.Domain.Entities;

namespace TunnlR.Application.Services.UrlHanlder
{
    public class TunnelUrlHandler : ITunnelUrlHandler
    {
        private readonly ITunnelService _tunnelService;
        private readonly IWebSocketConnectionManager _wsManager;

        public TunnelUrlHandler(ITunnelService tunnelService, IWebSocketConnectionManager wsManager)
        {
            _tunnelService = tunnelService;
            _wsManager = wsManager;
        }

        public async Task<WebSocket?> GetActiveSocketAsync(Guid tunnelId)
        {
            await Task.Yield();
            // Try to get WebSocket connection for tunnel
            var socket =  _wsManager.GetConnection(tunnelId);

            // Only return it if open, otherwise null
            if (socket?.State == WebSocketState.Open)
                return socket;
            return null;
        }

        public string GetLocalPath(PathString path)
        {
            // Split path into segments like before
            var parts = path.Value?.TrimStart('/').Split('/');

            // If there are more than 3 segments (api/tunnel/<id>/somethingelse, it definetiely means it is for testing)
            if (parts?.Length > 3)
            {
                // Rejoin all segments AFTER the first three
                // parts[3..] is the C# 8+ range operator: everything from index 3 to the end
                // Example: ["api","tunnel","GUID","pong"] → parts[3..] = ["pong"]
                return "/" + string.Join("/", parts[3..]);
            }

            // If no extra segments, return "/" as default path
            //return "/";
            throw new KeyNotFoundException("Local path not found after tunnel ID"); // middleware can catch this and return 404
        }

        public async Task<Tunnel?> GetTunnelForIdAsync(Guid tunnelId)
        {
            // Just fetch tunnel by ID
            return await _tunnelService.GetTunnelById(tunnelId);
        }

        public async Task<GetTunnelResponse?> GetTunnelForSubdomainAsync(string host)
        {
            // Get first part of host as subdomain
            // Example: "demo.example.com" → subdomain = "demo"
            var subdomain = host.Split('.')[0];

            // Fetch tunnel associated with that subdomain
            return await _tunnelService.GetTunnelBySubDomain(subdomain);
        }

        public bool IsSubdomainValid(string host)
        {
            // Split host by "." to get subdomain and domain parts
            // Example: "demo.example.com" → ["demo", "example", "com"]
            var parts = host.Split('.');

            // Valid subdomain exists if there are at least 3 parts
            // and the first part (subdomain) is not empty
            return parts.Length >= 3 && !string.IsNullOrEmpty(parts[0]);
        }

        public bool TryGetTunnelId(PathString path, out Guid tunnelId)
        {
            tunnelId = Guid.Empty;
            // Remove leading '/' and split the path into parts
            // Example: "/api/tunnel/0676356F-6183-4B36-8175-2F87D21CDDCF/pong"
            // becomes ["api", "tunnel", "0676356F-6183-4B36-8175-2F87D21CDDCF", "pong"]
            var parts = path.Value?.TrimStart('/').Split('/');


            // Checks if the path has at least 3 segments and starts with "api/tunnel"
            // This is the mian convention for local testing URLs
            if (parts?.Length >= 3 && parts[0] == "api" && parts[1] == "tunnel")
            {
                // Try to parse the third segment as a Guid (the tunnel ID)
                return Guid.TryParse(parts[2], out tunnelId);
            }

            // Not a local testing URL
            return false;
        }
    }
}
