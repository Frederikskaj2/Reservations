using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Frederikskaj2.Reservations.Persistence;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistenceInfrastructure(this IServiceCollection services) =>
        services
            .AddSingleton<EntityReaderWriter>()
            .AddSingleton<IEntityReader>(provider => provider.GetRequiredService<EntityReaderWriter>())
            .AddSingleton<IEntityWriter>(provider => provider.GetRequiredService<EntityReaderWriter>())
            .AddTransient<DatabaseInitializer>()
            .AddOption<CosmosOptions>()
            .AddTransient<IConfigureOptions<CosmosOptions>, ConfigureCosmosOptions>();

    class ConfigureCosmosOptions(IOptions<JsonSerializerOptions> serializerOptions) : IConfigureOptions<CosmosOptions>
    {
        readonly JsonSerializerOptions serializerOptions = serializerOptions.Value;

        public void Configure(CosmosOptions options) =>
            options.SerializerOptions = serializerOptions;
    }
}
