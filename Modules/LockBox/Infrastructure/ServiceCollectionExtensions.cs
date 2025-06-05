using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Frederikskaj2.Reservations.LockBox;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLockBoxInfrastructure(this IServiceCollection services) =>
        services
            .AddScoped<ILockBoxEmailService, LockBoxEmailService>()
            .AddJob<UpdateLockBoxCodesJobRegistration, UpdateLockBoxCodesJob>();
}
