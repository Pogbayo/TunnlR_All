using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;
using TunnlR.RelayServer.Persistence;

namespace TunnlR.Infrastructure.DesignTime
{
    public class TunnelDbContextFactory : IDesignTimeDbContextFactory<TunnelDbContext>
    {
        public TunnelDbContext CreateDbContext(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? "Server=.\\SQLEXPRESS;Database=TunnlR;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;";

            var optionsBuilder = new DbContextOptionsBuilder<TunnelDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new TunnelDbContext(optionsBuilder.Options);
        }
    }
}
