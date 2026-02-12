using Microsoft.Extensions.DependencyInjection;
using TunnlR.Application.Interfaces.IService;
using TunnlR.Application.Services.Auth;
using TunnlR.Application.Services.TunnelServices;
using TunnlR.Application.Services.UrlHanlder;
using TunnlR.Application.Services.WebSocketConnection;

namespace TunnlR.Application.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticationService, AuthenticationService>();
            services.AddScoped<ITunnelService, TunnelService>();
            services.AddScoped<ITunnelUrlHandler, TunnelUrlHandler>();
            services.AddSingleton<IWebSocketConnectionManager, WebSocketConnectionManager>();

            return services;
        }
    }
}
