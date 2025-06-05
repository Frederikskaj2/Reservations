using Microsoft.Extensions.DependencyInjection;
using OneOf.Serialization.SystemTextJson;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Orders;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersDomain(this IServiceCollection services) =>
        services
            .AddSingleton<JsonConverter, OneOfJsonConverter>()
            .AddSingleton<JsonConverter, OneOfBaseJsonConverter>();
}
