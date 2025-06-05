using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using System.Globalization;

namespace Frederikskaj2.Reservations.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCoreSharedInfrastructure(this IServiceCollection services)
    {
        // Blazor WASM by default does not provide a way to access a specific
        // culture. Instead, everything needed is configured here.
        var cultureInfo = new CultureInfo("da-DK")
        {
            DateTimeFormat =
            {
                DateSeparator = "-",
                TimeSeparator = ":",
                DayNames =
                [
                    "søndag",
                    "mandag",
                    "tirsdag",
                    "onsdag",
                    "torsdag",
                    "fredag",
                    "lørdag",
                ],
                MonthNames =
                [
                    "januar",
                    "februar",
                    "marts",
                    "april",
                    "maj",
                    "juni",
                    "juli",
                    "august",
                    "september",
                    "oktober",
                    "november",
                    "december",
                    "",
                ],
            },
            NumberFormat =
            {
                // kr. 123
                CurrencyPositivePattern = 2,
                // kr. -123
                CurrencyNegativePattern = 12,
            },
        };
        return services
            .AddSingleton(cultureInfo)
            .AddSingleton<IClock>(SystemClock.Instance)
            .AddSingleton(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!)
            .AddSingleton<ITimeConverter, TimeConverter>();
    }
}
