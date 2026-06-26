
// ================================================================
// FILE: API/HRMS.API/Extensions/JwtExtensions.cs  (ADD to existing Extensions folder)
// WHY: Wire JWT bearer validation into the ASP.NET pipeline
// Call this in Startup.ConfigureServices BEFORE AddGraphQL
// ================================================================
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
 
namespace HRMS.API.Extensions
{
    public static class JwtExtensions
    {
        public static IServiceCollection AddJwtAuthentication(
            this IServiceCollection services, IConfiguration config)
        {
            var secretKey = config["Jwt:SecretKey"]
                ?? throw new InvalidOperationException("Jwt:SecretKey is not configured");
 
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    ValidateIssuer           = true,
                    ValidIssuer              = config["Jwt:Issuer"],
                    ValidateAudience         = true,
                    ValidAudience            = config["Jwt:Audience"],
                    ValidateLifetime         = true,
                    ClockSkew                = TimeSpan.Zero  // No tolerance for expired tokens
                };
 
                // Allow JWT from GraphQL requests too
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
                        if (authHeader?.StartsWith("Bearer ") == true)
                            ctx.Token = authHeader["Bearer ".Length..].Trim();
                        return Task.CompletedTask;
                    }
                };
            });
 
            services.AddAuthorization(options =>
            {
                options.AddPolicy("RequireAuth",         p => p.RequireAuthenticatedUser());
                options.AddPolicy("RequireHR",           p => p.RequireRole("SuperAdmin", "HR"));
                options.AddPolicy("RequireManager",      p => p.RequireRole("SuperAdmin", "HR", "Manager"));
                options.AddPolicy("RequireSuperAdmin",   p => p.RequireRole("SuperAdmin"));
            });
 
            return services;
        }
    }
}
 