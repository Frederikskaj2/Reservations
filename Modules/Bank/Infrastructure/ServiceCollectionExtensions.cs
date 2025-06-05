using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Frederikskaj2.Reservations.Bank;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBankInfrastructure(this IServiceCollection services) =>
        services
            .AddScoped<IBankEmailService, BankEmailService>()
            .AddSingleton<IBankTransactionsParser, BankTransactionsParser>()
            .AddJob<SendDebtRemindersJobRegistration, SendDebtRemindersJob>();
}
