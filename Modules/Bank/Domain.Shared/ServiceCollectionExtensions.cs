using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Bank;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBankSharedDomain(this IServiceCollection services) =>
        services
            .AddSingleton<JsonConverter, BankTransactionIdConverter>()
            .AddSingleton<JsonConverter, PayOutIdConverter>();
}
