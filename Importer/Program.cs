using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Importer.Input;
using Frederikskaj2.Reservations.Infrastructure;
using Frederikskaj2.Reservations.Infrastructure.Persistence;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Importer;

static class Program
{
    public static Task Main(string[] args)
    {
        const string connectionString = "Data Source=../../../../Reservations.db";
        var cultureInfo = new CultureInfo("da-DK")
        {
            DateTimeFormat =
            {
                // These have different values in Blazor WASM so are set
                // here for consistency.
                DateSeparator = "-",
                TimeSeparator = ":"
            },
            NumberFormat =
            {
                // kr. 123
                CurrencyPositivePattern = 2,
                // kr. -123
                CurrencyNegativePattern = 12
            }
        };
        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(builder => builder.AddJsonFile("appsettings.local.json", true, true))
            .ConfigureServices(services => services
                .AddDbContext<ReservationsContext>(options => options.UseSqlite(connectionString))
                .AddSerialization()
                .AddJsonConverters()
                .AddCosmos()
                .AddOption<CosmosOptions>()
                .AddOption<ApplicationOptions>()
                .AddTransient<IOptionsFactory<OrderingOptions>, OrderingOptionsFactory>()
                .AddSingleton(cultureInfo)
                .AddSingleton<IFormatter, Formatter>()
                .AddHostedService<Importer>());
        return builder.RunConsoleAsync();
    }
}
