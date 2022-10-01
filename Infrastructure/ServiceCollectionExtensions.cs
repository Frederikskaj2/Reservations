using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Infrastructure.Persistence;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Frederikskaj2.Reservations.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services) =>
        services
            .AddMemoryCache()
            .AddTransient<IConfigureOptions<MemoryCacheOptions>, ConfigureMemoryCacheOptions>()
            .AddHttpClient()
            .AddSingleton<IEmailQueue, EmailQueue>()
            .AddSingleton<IEncryptedTokenProvider, EncryptedTokenProvider>()
            .AddSingleton<IPasswordHasher, PasswordHasher>()
            .AddSingleton<IPasswordValidator, PasswordValidator>()
            .AddSingleton<IRandomNumberGenerator, RandomNumberGenerator>()
            .AddSingleton<IRemotePasswordChecker, RemotePasswordChecker>();

    public static IServiceCollection AddCosmos(this IServiceCollection services) =>
        services
            .AddSingleton<IPersistenceContextFactory, Persistence.Cosmos>()
            .AddTransient<IConfigureOptions<CosmosOptions>, ConfigureCosmosOptions>();

    class ConfigureCosmosOptions : IConfigureOptions<CosmosOptions>
    {
        readonly JsonSerializerOptions serializerOptions;

        public ConfigureCosmosOptions(IOptions<JsonSerializerOptions> serializerOptions) =>
            this.serializerOptions = serializerOptions.Value;

        public void Configure(CosmosOptions options) =>
            options.SerializerOptions = serializerOptions;
    }

    class ConfigureMemoryCacheOptions : IConfigureOptions<MemoryCacheOptions>
    {
        readonly PasswordOptions passwordOptions;

        public ConfigureMemoryCacheOptions(IOptions<PasswordOptions> options) => passwordOptions = options.Value;

        public void Configure(MemoryCacheOptions options) => options.SizeLimit = passwordOptions.RemoteChecker.CacheSize;
    }
}
