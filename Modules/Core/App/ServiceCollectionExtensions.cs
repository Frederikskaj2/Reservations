using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Frederikskaj2.Reservations.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreApp(this IServiceCollection services) =>
        services.AddTransient<IConfigureOptions<JsonOptions>, ConfigureJsonOptions>();

    class ConfigureJsonOptions(IConfigureOptions<JsonSerializerOptions> configureJsonSerializerOptions) : IConfigureOptions<JsonOptions>
    {
        public void Configure(JsonOptions options) =>
            configureJsonSerializerOptions.Configure(options.SerializerOptions);
    }
}
