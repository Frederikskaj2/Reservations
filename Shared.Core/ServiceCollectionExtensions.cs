using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NodaTime;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddShared(this IServiceCollection services)
    {
        // Blazor WASM by default does not provide a way to access a specific
        // culture. Instead everything needed is configured here.
        var cultureInfo = new CultureInfo("da-DK")
        {
            DateTimeFormat =
            {
                DateSeparator = "-",
                TimeSeparator = ":",
                DayNames = new[]
                {
                    "søndag",
                    "mandag",
                    "tirsdag",
                    "onsdag",
                    "torsdag",
                    "fredag",
                    "lørdag"
                },
                MonthNames = new[]
                {
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
                    ""
                }
            },
            NumberFormat =
            {
                // kr. 123
                CurrencyPositivePattern = 2,
                // kr. -123
                CurrencyNegativePattern = 12
            }
        };
        return services
            .AddSingleton(cultureInfo)
            .AddSingleton<IClock>(SystemClock.Instance)
            .AddSingleton(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!)
            .AddScoped<IDateProvider, DateProvider>();
    }

    public static IServiceCollection AddSerialization(this IServiceCollection services)
        => services
            .AddSingleton<JsonConverter, AmountConverter>()
            .AddSingleton<JsonConverter, ApartmentIdConverter>()
            .AddSingleton<JsonConverter, DeviceIdConverter>()
            .AddSingleton<JsonConverter, DurationConverter>()
            .AddSingleton<JsonConverter, EmailAddressConverter>()
            .AddSingleton<JsonConverter, IntKeyDictionaryConverter>()
            .AddSingleton<JsonConverter, InstantConverter>()
            .AddSingleton<JsonConverter, LocalTimeConverter>()
            .AddSingleton<JsonConverter, LocalDateConverter>()
            .AddSingleton<JsonConverter, LocalDateTimeConverter>()
            .AddSingleton<JsonConverter, OrderIdConverter>()
            .AddSingleton<JsonConverter, PaymentIdConverter>()
            .AddSingleton<JsonConverter, PeriodConverter>()
            .AddSingleton<JsonConverter, ReservationIdConverter>()
            .AddSingleton<JsonConverter, ReservationIndexConverter>()
            .AddSingleton<JsonConverter, ResourceIdConverter>()
            .AddSingleton<JsonConverter, TokenIdConverter>()
            .AddSingleton<JsonConverter, TransactionIdConverter>()
            .AddSingleton<JsonConverter, UserIdConverter>()
            .AddTransient<IConfigureOptions<JsonSerializerOptions>, ConfigureJsonSerializerOptions>();

    class ConfigureJsonSerializerOptions : IConfigureOptions<JsonSerializerOptions>
    {
        readonly IEnumerable<JsonConverter> converters;

        public ConfigureJsonSerializerOptions(IEnumerable<JsonConverter> converters) => this.converters = converters;

        public void Configure(JsonSerializerOptions options)
        {
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
            options.PropertyNameCaseInsensitive = true;
            foreach (var converter in converters)
                options.Converters.Add(converter);
        }
    }
}
