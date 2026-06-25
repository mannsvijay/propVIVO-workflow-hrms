using HRMS.API;
using HRMS.Core.KeyVault.Extensions;
using Microsoft.AspNetCore.Http.Timeouts;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

builder.Configuration.AddAzureKeyVaultConfiguration(builder.Configuration);

// Request timeout: configurable via RequestTimeout:Seconds (default 90). Used by ASP.NET, Kestrel,
// and GraphQL.
int requestTimeoutSeconds = builder.Configuration.GetValue<int>("RequestTimeout:Seconds", 90);

builder.Services.AddRequestTimeouts(options =>
{
    options.DefaultPolicy = new RequestTimeoutPolicy
    {
        Timeout = TimeSpan.FromSeconds(requestTimeoutSeconds),
        TimeoutStatusCode = StatusCodes.Status504GatewayTimeout
    };
});

// Register IHttpContextAccessor for GraphQL telemetry initializer
builder.Services.AddHttpContextAccessor();

builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});

// Configure custom sampling telemetry processor to reduce costs significantly (10% sampling = 90%
// cost reduction)
//builder.Services.AddApplicationInsightsTelemetryProcessor<SamplingTelemetryProcessor>();

// Ensure TelemetryClient is available for DI.
//builder.Services.AddSingleton<TelemetryClient>();

// Configure Kestrel: larger request body and configurable timeouts (RequestTimeout:Seconds)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 105 * 1024 * 1024;
    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(requestTimeoutSeconds);
    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(Math.Max(requestTimeoutSeconds + 30, 120));
});

// Add services to the container.
var startup = new Startup();
startup.ConfigureServices(builder.Services, builder.Configuration);

var app = builder.Build();
app.Logger.LogInformation($"Current environment: {app.Environment.EnvironmentName}");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

startup.Configure(app, app.Environment, builder.Configuration);
app.Run();