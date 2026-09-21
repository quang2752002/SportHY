using Dms.Application.Interfaces;
using Dms.Domain.Interfaces;
using Dms.Infrastructure.Persistence;
using Dms.Infrastructure.Repositories;
using Dms.Infrastructure.Services;
using Dms.Infrastructure.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dms.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            // Authentication Services
            services.AddScoped<ITokenService,TokenService>();
            services.AddScoped<IGoogleAuthService,GoogleAuthService>();
            
            // Recaptcha Service
            services.AddHttpClient<IRecaptchaService,RecaptchaService>();

            // Custom Authorization Permission Policy Provider & Handler
            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

            return services;
        }
    }
}
