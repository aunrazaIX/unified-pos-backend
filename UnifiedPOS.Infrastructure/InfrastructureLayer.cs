using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UnifiedPOS.Application.Interfaces.RepositoryInterface;
using UnifiedPOS.Infrastructure.Databases;

namespace UnifiedPOS.Infrastructure
{
    public static class InfrastructureLayer
    {
        public static IServiceCollection AddInfrastructureLayer(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("POSConnection");
            if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("ConnectionStrings:POSConnection is required.");
            services.AddDbContext<POSContext>(options => options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure()));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            return services;
        }
    }
}
