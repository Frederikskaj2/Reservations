using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.LockBox;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLockBoxSharedDomain(this IServiceCollection services) =>
        services
            .AddSingleton<JsonConverter, ResourceIdConverter>();
}
