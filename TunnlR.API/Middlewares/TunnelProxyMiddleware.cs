using System.IO;
using System.Net.WebSockets;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using TunnlR.API.WebSockets;
using TunnlR.Application.Interfaces.IService;

namespace TunnlR.API.Middlewares
{
    public class TunnelProxyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IServiceProvider _serviceProvider; 

        public TunnelProxyMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
        {
            _next = next;
            _serviceProvider = serviceProvider;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            // Resolve scoped services from the scope
            var _wsManager = serviceProvider.GetRequiredService<IWebSocketConnectionManager>();
            var _urlHandler = serviceProvider.GetRequiredService<ITunnelUrlHandler>();

            // Step 1: Extract host and path
            var host = context.Request.Host.Host; // locahlost or subdomain
            var path = context.Request.Path;

            var requestId = Guid.NewGuid(); // Unique request ID
            var localPath = string.Empty;

            Console.WriteLine("Got to the TunnelMiddleware entrance");
            // Step 1: Check for local test tunnel request

            if (_urlHandler.TryGetTunnelId(path, out var tunnelId))
            {
                Console.WriteLine("Entered local test block");
                Console.WriteLine($"TunnelId parsed: {tunnelId}");

                localPath = _urlHandler.GetLocalPath(path);
                Console.WriteLine($"Local path extracted: {localPath}");

                Console.WriteLine("Starting SerializeHttpRequest...");
                var requestData = await SerializeHttpRequest(context.Request, localPath, requestId);
                Console.WriteLine("SerializeHttpRequest finished");

                Console.WriteLine("Fetching socket...");
                var socket = await _urlHandler.GetActiveSocketAsync(tunnelId);
                Console.WriteLine($"Socket fetched: {(socket == null ? "NULL" : socket.State.ToString())}");

                if (socket == null || socket.State != WebSocketState.Open)
                {
                    Console.WriteLine("Socket invalid/offline → returning 503");
                    context.Response.StatusCode = 503;
                    await context.Response.WriteAsync("Tunnel offline");
                    return;
                }

                Console.WriteLine("Sending message to CLI...");
                await _wsManager.SendMessageAsync(tunnelId, requestData);
                Console.WriteLine("Message sent");

                try
                {
                    Console.WriteLine("Waiting for response (30s timeout)...");
                    var cliResponse = await TunnelWebSocketHandler.WaitForResponse(requestId, TimeSpan.FromSeconds(30));
                    Console.WriteLine("Response received from CLI");
                    context.Response.StatusCode = 200;
                    await context.Response.WriteAsync(cliResponse);
                    Console.WriteLine("Response written to browser");
                }
                catch (TimeoutException )
                {
                    Console.WriteLine("TimeoutException caught");
                    context.Response.StatusCode = 504;
                    await context.Response.WriteAsync("Gateway timeout");
                    Console.WriteLine("Timeout response written");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unexpected error in wait block: {ex.Message}");
                    context.Response.StatusCode = 500;
                    await context.Response.WriteAsync("Internal error");
                }
                return;
            }

            // Step 2: Subdomain tunnel request
            if (_urlHandler.IsSubdomainValid(host))
            {
                //fetching the tunnel from the subdomain in the host
                var tunnel = await _urlHandler.GetTunnelForSubdomainAsync(host);
                localPath = _urlHandler.GetLocalPath(path);
                //checks for null tunnels
                if (tunnel == null)
                {
                    //if null, write the statuscode to 404 which the message "" TUnnel not found
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Tunnel not found");
                    return;
                }

                //if tunnel status is not active, drop it and write a message for it
                if (tunnel.Status != Contract.DTOs.Enums.TunnelStatus.Active)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync("Tunnel is inactive");
                    return;
                }
                //fetches the ActiveSocket by tunnelId
                var socket = await _urlHandler.GetActiveSocketAsync(tunnel.TunnelId);
                if (socket == null)
                {
                    //if socket is null, run this code
                    context.Response.StatusCode = 503;
                    await context.Response.WriteAsync("Tunnel offline");
                    return;
                }
                var requestData = await SerializeHttpRequest(context.Request,localPath, requestId);

                //sends the data to the cli
                await _wsManager.SendMessageAsync(tunnel.TunnelId, requestData);

                try
                {
                    //once again the middleware has to wait for the response befor writing ot the context reposne body so it has to wait for the response here
                    var cliResponse = await TunnelWebSocketHandler.WaitForResponse(requestId, TimeSpan.FromSeconds(30));
                    context.Response.StatusCode = 200;
                    //writes the response body here
                    await context.Response.WriteAsync(cliResponse);
                }
                catch (TimeoutException)
                {
                    context.Response.StatusCode = 504;
                    await context.Response.WriteAsync("Gateway timeout");
                }

                return;
            }

            await _next(context);
        }

        private async Task<string> SerializeHttpRequest(HttpRequest request,string localPath, Guid requestId)
        {
            return JsonSerializer.Serialize(new
            {
                RequestId = requestId,
                Method = request.Method,
                Path = localPath.ToString(),
                Headers = request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString()),
                Body = await ReadBodyAsync(request)
            });
        }

        private async Task<string> ReadBodyAsync(HttpRequest request)
        {
            request.EnableBuffering();
            using var reader = new StreamReader(request.Body);
            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0; // reset stream for downstream middelware
            return body;
        }
    }
}
