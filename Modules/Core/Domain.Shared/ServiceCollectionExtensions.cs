using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Frederikskaj2.Reservations.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreSharedDomain(this IServiceCollection services) =>
        services
            .AddSingleton<JsonConverter, DurationConverter>()
            .AddSingleton<JsonConverter, ETagConverter>()
            .AddSingleton<JsonConverter, InstantConverter>()
            .AddSingleton<JsonConverter, LocalDateConverter>()
            .AddSingleton<JsonConverter, LocalDateTimeConverter>()
            .AddSingleton<JsonConverter, LocalTimeConverter>()
            .AddSingleton<JsonConverter, PeriodConverter>()
            .AddTransient<IConfigureOptions<JsonSerializerOptions>, ConfigureJsonSerializerOptions>();

    class ConfigureJsonSerializerOptions(IEnumerable<JsonConverter> converters, IJsonTypeInfoResolver typeInfoResolver)
        : IConfigureOptions<JsonSerializerOptions>
    {
        public void Configure(JsonSerializerOptions options)
        {
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            foreach (var converter in converters)
                options.Converters.Add(converter);
            options.TypeInfoResolver = typeInfoResolver;
        }
    }
}
