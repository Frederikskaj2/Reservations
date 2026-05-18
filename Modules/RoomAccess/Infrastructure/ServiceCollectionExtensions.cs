using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Net.Mime;

namespace Frederikskaj2.Reservations.RoomAccess;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddRoomAccessInfrastructure(this IServiceCollection services)
    {
        services
            .AddOption<NukiOptions>()
            .AddScoped<IRoomAccessEmailService, RoomAccessEmailService>()
            .AddJob<SendRoomEntryCodesJobRegistration, SendRoomEntryCodesJob>()
            .AddJob<UpdateSmartLockAuthorizationsJobRegistration, UpdateSmartLockAuthorizationsJob>()
            .AddHttpClient<ISmartLockService, NukiSmartLockService>((serviceProvider, client) =>
            {
                var options = serviceProvider.GetRequiredService<IOptions<NukiOptions>>();
                client.BaseAddress = options.Value.Endpoint;
                client.DefaultRequestHeaders.Accept.Add(new(MediaTypeNames.Application.Json));
                client.DefaultRequestHeaders.Authorization = new("Bearer", options.Value.ApiKey ?? throw new ConfigurationException("Missing Nuki API Key."));
            });
        return services;
    }
}
