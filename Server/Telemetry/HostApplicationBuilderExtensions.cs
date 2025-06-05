using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Frederikskaj2.Reservations.Server;

static class HostApplicationBuilderExtensions
{
    public static IHostApplicationBuilder AddTelemetry(this IHostApplicationBuilder builder, IWebHostEnvironment hostEnvironment)
    {
        if (hostEnvironment.EnvironmentName is "IntegrationTest")
            return builder;
        var telemetryOptions = builder.Configuration.GetSection("Telemetry").Get<TelemetryOptions>();
        if (telemetryOptions?.IsEnabled ?? false)
            builder.Services.AddOpenTelemetry().UseAzureMonitor(options =>
            {
                options.ConnectionString = telemetryOptions.ConnectionString;
                options.SamplingRatio = 0F;
            });
        return builder;
    }
}
