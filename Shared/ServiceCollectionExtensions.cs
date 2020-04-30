using System;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddReservationsServices(this IServiceCollection services)
        {
            if (services is null)
                throw new ArgumentNullException(nameof(services));

            return services
                .AddSingleton<IClock>(SystemClock.Instance)
                .AddSingleton(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen"))
                .AddSingleton<IDateProvider, DateProvider>()
                .AddSingleton<ReservationsOptions>()
                .AddSingleton<HolidaysProvider>()
                .AddScoped<BanquetFacilitiesReservationPolicy>()
                .AddScoped<BedroomReservationPolicy>()
                .AddScoped<IReservationPolicyProvider, ReservationPolicyProvider>();
        }
    }
}