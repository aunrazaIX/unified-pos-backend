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
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IOrganizationService, OrganizationService>();
            services.AddScoped<ICatalogService, CatalogService>();
            services.AddScoped<IRegisterService, RegisterService>();
            services.AddScoped<ICheckoutService, CheckoutService>();
            services.AddScoped<ISalesService, SalesService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IPosService, PosService>();
            return services;
        }
    }
}
