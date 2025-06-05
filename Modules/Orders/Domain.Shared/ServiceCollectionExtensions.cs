using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Orders;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersSharedDomain(this IServiceCollection services) =>
        services
            .AddSingleton<JsonConverter, ReservationIndexConverter>();
}
