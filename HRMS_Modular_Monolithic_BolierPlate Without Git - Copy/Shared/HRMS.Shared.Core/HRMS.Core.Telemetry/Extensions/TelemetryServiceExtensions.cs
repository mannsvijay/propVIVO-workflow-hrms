using Microsoft.Extensions.DependencyInjection;

namespace HRMS.Core.Telemetry.Extensions
{
    /// <summary>
    /// Extension methods for registering TelemetryService in dependency injection
    /// </summary>
    public static class TelemetryServiceExtensions
    {
        /// <summary>
        /// Adds TelemetryService as a singleton to the service collection
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddTelemetryService(this IServiceCollection services)
        {
            services.AddSingleton<ITelemetryService, TelemetryService>();
            return services;
        }
    }
}