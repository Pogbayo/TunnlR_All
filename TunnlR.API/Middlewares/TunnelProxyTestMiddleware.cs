// TunnelProxyTestMiddleware.cs
using System;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using TunnlR.API.WebSockets;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Contract.DTOs.Auth;
using TunnlR.Domain.Entities;

namespace TunnlR.API.Middlewares
{
    public class TunnelProxyTestMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWebSocketConnectionManager _wsManager;

        public TunnelProxyTestMiddleware(
            RequestDelegate next,
            IServiceProvider serviceProvider,
            IWebSocketConnectionManager wsManager)
        {
            _next = next;
            _serviceProvider = serviceProvider;
            _wsManager = wsManager;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.TrimStart('/') ?? "";
            Console.WriteLine($"Middleware hit - Path: {path}");

            if (!path.StartsWith("api/tunnel/"))
            {
                await _next(context);
                return;
            }
            Console.WriteLine("Entered local test block");
            var pathParts = path.Split('/');
            if (pathParts.Length < 2 || !Guid.TryParse(pathParts[2], out var tunnelId))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid tunnel ID in test path");
                return;
            }

            using var scope = _serviceProvider.CreateScope();
            var tunnelService = scope.ServiceProvider.GetRequiredService<ITunnelService>();

            var tunnel = await tunnelService.GetTunnelById(tunnelId);
            if (tunnel == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Tunnel not found");
                return;
            }

            if (tunnel.Status != Domain.DTOs.Enums.TunnelStatus.Active)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync("Tunnel is inactive");
                return;
            }

            var socket = _wsManager.GetConnection(tunnel.Id);
            Console.WriteLine($"Socket for tunnel {tunnel.Id}: {(socket == null ? "NULL" : socket.State.ToString())}");
            if (socket?.State != WebSocketState.Open)
            {
                context.Response.StatusCode = 503;
                await context.Response.WriteAsync("Tunnel offline");
                return;
            }

            var requestId = Guid.NewGuid();
            var requestData = await SerializeHttpRequest(context.Request, requestId);
            await _wsManager.SendMessageAsync(tunnel.Id, requestData);

            try
            {
                var cliResponse = await TunnelWebSocketHandler.WaitForResponse(
                    requestId,
                    TimeSpan.FromSeconds(30)
                );

                //var responseData = JsonSerializer.Deserialize<HttpResponseData>(cliResponse);

                //context.Response.StatusCode = responseData!.StatusCode;

                //foreach (var header in responseData.Headers)
                //{
                //    if (!header.Key.StartsWith(":")) 
                //    {
                //        context.Response.Headers[header.Key] = header.Value;
                //    }
                //}

                //await context.Response.WriteAsync(responseData.Body);

                context.Response.StatusCode = 200;
                await context.Response.WriteAsync(cliResponse);
            }
            catch (TimeoutException)
            {
                context.Response.StatusCode = 504;
                await context.Response.WriteAsync("Gateaway timeout");
            }
        }

        private async Task<string> SerializeHttpRequest(HttpRequest request, Guid requestId)
        {
            return JsonSerializer.Serialize(new
            {
                RequestId = requestId,
                Method = request.Method,
                Path = request.Path.ToString(),
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