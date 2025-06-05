using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Frederikskaj2.Reservations.Orders;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOrdersInfrastructure(this IServiceCollection services) =>
        services
            .AddOption<OrderingOptions>()
            .AddScoped<IOrdersEmailService, OrdersEmailService>()
            .AddJob<ConfirmOrdersJobRegistration, ConfirmOrdersJob>()
            .AddJob<FinishOwnerOrdersJobRegistration, FinishOwnerOrdersJob>()
            .AddJob<RemoveAccountNumbersJobRegistration, RemoveAccountNumbersJob>()
            .AddJob<SendLockBoxCodesJobRegistration, SendLockBoxCodesJob>()
            .AddJob<SendSettlementNeededRemindersJobRegistration, SendSettlementNeededRemindersJob>();
}
