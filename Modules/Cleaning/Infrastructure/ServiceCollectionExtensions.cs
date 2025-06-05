using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Frederikskaj2.Reservations.Cleaning;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCleaningInfrastructure(this IServiceCollection services) =>
        services
            .AddScoped<ICleaningEmailService, CleaningEmailService>()
            .AddJob<SendCleaningScheduleUpdateJobRegistration, SendCleaningScheduleUpdateJob>()
            .AddJob<UpdateCleaningScheduleJobRegistration, UpdateCleaningScheduleJob>();
}
