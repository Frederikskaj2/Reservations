using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Frederikskaj2.Reservations.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreInfrastructure(this IServiceCollection services) =>
        services
            .AddOption<DateProviderOptions>()
            .AddOption<JobSchedulerOptions>()
            .AddSingleton<IJobScheduler, JobScheduler>()
            .AddHostedService(serviceProvider => (JobScheduler) serviceProvider.GetRequiredService<IJobScheduler>())
            .AddSingleton<JsonConverter, HashMapConverter>()
            .AddSingleton<JsonConverter, OptionConverter>()
            .AddSingleton<JsonConverter, SeqConverter>()
            .AddSingleton<IJsonTypeInfoResolver, OptionAwareJsonTypeInfoResolver>()
            .AddScoped<IDateProvider, DateProvider>();

    public static IServiceCollection AddJob<TJobRegistration, TJob>(this IServiceCollection services)
        where TJobRegistration : class, IJobRegistration where TJob : class, IJob =>
        services
            .AddSingleton<IJobRegistration, TJobRegistration>()
            .AddScoped<TJob>();
}
