
// ================================================================
// FILE: Modules/AuthFeature/AuthFeature.Infrastructure/ConfigureServiceExtension.cs
// WHY: Register all AuthFeature services in DI container
// ================================================================
using AuthFeature.Application.Repository;
using AuthFeature.Application.Services;
using Microsoft.Extensions.DependencyInjection;
 
namespace AuthFeature.Infrastructure
{
    public static class ConfigureServiceExtension
    {
        public static IServiceCollection AddAuthFeature(this IServiceCollection services)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IJwtService, JwtService>();
            services.AddScoped<IPasswordService, PasswordService>();
            return services;
        }
    }
}
 
 
// ================================================================
// APPSETTINGS additions — add to appsettings.json under existing keys
// ================================================================
/*
  "Jwt": {
    "SecretKey": "propVivo-HRMS-SuperSecret-Key-At-Least-32-Chars-Long!",
    "Issuer": "propvivo-hrms-api",
    "Audience": "propvivo-hrms-frontend",
    "AccessTokenMinutes": "60"
  },
  "Cors": {
    "AllowedOrigins": ["http://localhost:3000", "https://your-production-domain.com"]
  }
*/
 