using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using UnifiedPOS.Application.Interfaces.ServiceInterface;
using UnifiedPOS.Application.Services;
using UnifiedPOS.Domain.Entities;

namespace UnifiedPOS.Application
{
    public static class ApplicationLayer
    {
        public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<UserService>();
            services.AddScoped<OrganizationService>();
            services.AddScoped<CatalogService>();
            services.AddScoped<RegisterService>();
            services.AddScoped<CheckoutService>();
            services.AddScoped<SalesService>();
            services.AddScoped<CustomerService>();
            services.AddScoped<ReportService>();
            services.AddScoped<PosService>();
            return services;
        }
    }
}
