using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TunnlR.Domain.Entities;
using TunnlR.Infrastructure.TokenAuthentication;
using TunnlR.RelayServer.Persistence;

namespace TunnlR.Infrastructure.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Database
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            services.AddDbContext<TunnelDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Identity
            services.AddIdentity<AppUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<TunnelDbContext>()
                .AddDefaultTokenProviders();

            //I just built a ci/cd pipeline to test bb
            //Token
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

            return services;
        }
    }
}