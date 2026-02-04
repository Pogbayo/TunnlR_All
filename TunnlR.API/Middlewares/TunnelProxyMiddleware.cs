using System.Net.WebSockets;
using System.Text.Json;
using Domain.Enums;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Application.Services.TunnelServices;

namespace TunnlR.API.Middlewares
{
    public class TunnelProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly TunnelService _tunnelService;
        private readonly IWebSocketConnectionManager _wsManager;

        public TunnelProxyMiddleware(RequestDelegate next, TunnelService tunnelService, IWebSocketConnectionManager wsmanager)
        {
            _next = next;
            _tunnelService = tunnelService;
            _wsManager = wsmanager; 
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var host = context.Request.Host.Host;
            var subdomain = host.Split('.')[0];

            if (string.IsNullOrEmpty(subdomain) || host == "tunnlr.dev")
            {
                await _next(context);
                return;
            }

            var tunnel = await _tunnelService.GetTunnelBySubDomain(subdomain);
            if (tunnel.Status != TunnelStatus.Active)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Tunnel is inactive");
                return;
            }

            var socket = _wsManager.GetConnection(tunnel.TunnelId);
            if (socket?.State != WebSocketState.Open)
            {
                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("Tunnel offline");
                return;

            }

            var requestData = await SerializeHttpRequest(context.Request);
            await _wsManager.SendMessageAsync(tunnel.TunnelId, requestData);

            //var cliResponse = await WaitForResponse(tunnelId);

            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("Request forwarded (placeholder)");
        }

        private async Task<string> SerializeHttpRequest(HttpRequest request)
        {
            return JsonSerializer.Serialize(new
            {
                Method = request.Method,
                Path = request.Path,
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = await ReadBodyAsync(request)
            });
        }

        private async Task<string> ReadBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body);
            return await reader.ReadToEndAsync();
        }
    }
}