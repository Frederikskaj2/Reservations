using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Users;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersSharedDomain(this IServiceCollection services) =>
        services
            .AddSingleton<JsonConverter, AmountConverter>()
            .AddSingleton<JsonConverter, ApartmentIdConverter>()
            .AddSingleton<JsonConverter, EmailAddressConverter>()
            .AddSingleton<JsonConverter, OrderIdConverter>()
            .AddSingleton<JsonConverter, PaymentIdConverter>()
            .AddSingleton<JsonConverter, TransactionIdConverter>()
            .AddSingleton<JsonConverter, UserIdConverter>();
}
