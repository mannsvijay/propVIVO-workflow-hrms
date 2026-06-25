using HRMS.Core.Postgres;
using HRMS.API.Extensions;
using HRMS.API.RegisterDependencies;
using HRMS.Shared.Infrastructure.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;
using System.Reflection;
using System.Text.Json.Serialization;
using TodoFeature.Application.DTO;

namespace HRMS.API
{
    public class NoCacheFilter : IActionFilter
    {
        public void OnActionExecuted(ActionExecutedContext context)
        {
            //we return in case of WebSockets
            if (!context.HttpContext.WebSockets.IsWebSocketRequest)
            {
                context.HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, max-age=0";
                context.HttpContext.Response.Headers["Pragma"] = "no-cache";
                context.HttpContext.Response.Headers["Expires"] = "-1";
            }
        }

        public void OnActionExecuting(ActionExecutingContext context)
        { }
    }

    public class Startup
    {
        public void Configure(WebApplication app, IWebHostEnvironment env, IConfiguration configuration)
        {
            app.UseForwardedHeaders();

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            _ = Task.Run(() =>
            {
                try
                {
                    app.EnsurePostgresDbIsCreated();
                    Console.WriteLine("PostgreSQL database initialized successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error initializing PostgreSQL database: {ex.Message}");
                }
            });

            app.UseRouting();
            app.UseRequestTimeouts();
            app.UseCors();
            app.AddMiddleware();
            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
            {
                context.Response.Headers.CacheControl = "no-store, no-cache, must-revalidate, max-age=0";
                context.Response.Headers.Pragma = "no-cache";
                context.Response.Headers.Expires = "-1";
                await next();
            });

            bool enableGraphQLTool = configuration.GetValue<bool>("GraphQL:Tool:Enable", env.IsDevelopment());

            app.MapControllers();
            app.MapGraphQL()
                .WithOptions(options =>
                {
                    options.Tool.Enable = enableGraphQLTool;
                });

            //app.UseVoyager(new VoyagerOptions
            //{
            //    Path = "/voyager",
            //});
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                // Replace with your actual reverse proxy / load balancer IP. For Azure App Gateway
                // or Front Door, add their published egress ranges to KnownNetworks instead of a
                // single IP: options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("10.0.0.0"), 8));
                options.KnownProxies.Add(IPAddress.Parse("10.0.0.1"));
            });

            services.AddControllers()
                   .AddJsonOptions(options =>
                   {
                       options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                   });

            services.AddEndpointsApiExplorer();

            // Register IHttpClientFactory for proper HttpClient management This prevents socket
            // exhaustion and memory leaks from creating new HttpClient instances
            services.AddHttpClient();

            services.AddInjectionApplication(configuration, [typeof(CreateTodoHandler).Assembly]);
            services.AddInjectionPostgres(configuration);
            services.AddModulesDependencyInjection(configuration);

            // Background services are not needed without websocket middleware.

            services.ConfigureApiBehavior();
            services.ConfigureCorsPolicy(configuration);
            services.ConfigureGraphQL(configuration);

            services.AddMemoryCache();
            services.AddMvc(options =>
            {
                options.Filters.Add(new NoCacheFilter());
            });

            //services.AddApiVersioning(config =>
            //{
            //    config.DefaultApiVersion = new ApiVersion(1, 0);
            //    config.AssumeDefaultVersionWhenUnspecified = true;
            //    config.ApiVersionReader = ApiVersionReader.Combine(
            //        new UrlSegmentApiVersionReader(),
            //        new QueryStringApiVersionReader("version"),
            //        new HeaderApiVersionReader("X-Version")
            //    );
            //});
        }
    }
}
