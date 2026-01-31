using Microsoft.Extensions.DependencyInjection;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Application.Services.Auth;
using TunnlR.Application.Services.Tunnel;
using TunnlR.Application.Services.WebSocket;

namespace TunnlR.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITunnelService, TunnelService>();
            services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();

            return services;
        }
    }
}
