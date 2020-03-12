using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Frederikskaj2.Reservations.Client
{
    [SuppressMessage(
        "Design", "RCS1102:Make class static.",
        Justification = "Blazor client side app cannot be hosted in ASP.NET Core website if it is static.")]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            ConfigureServices(builder.Services);
            var host = builder.Build();
            host.Services
                .UseBootstrapProviders()
                .UseFontAwesomeIcons();
            await host.RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            services
                .AddBlazorise()
                .AddBootstrapProviders()
                .AddFontAwesomeIcons();

            // See https://github.com/dotnet/aspnetcore/issues/18733#issuecomment-585817720
            services
                .AddAuthorizationCore(options => { })
                .AddScoped<ServerAuthenticationStateProvider>()
                .AddScoped<AuthenticationStateProvider>(
                    sp => sp.GetRequiredService<ServerAuthenticationStateProvider>())
                .AddScoped<IAuthenticationStateProvider>(
                    sp => sp.GetRequiredService<ServerAuthenticationStateProvider>());

            var jsonSerializerOptions = new JsonSerializerOptions
                {
                    IgnoreNullValues = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                }
                .ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);
            services
                .AddSingleton(jsonSerializerOptions)
                .AddSingleton<ApiClient>();

            services
                .AddSingleton(CultureInfo.GetCultureInfo("da-DK"))
                .AddSingleton(DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen"))
                .AddSingleton<FormattingService>()
                .AddReservationsServices()
                .AddSingleton<ClientDataProvider>()
                .AddSingleton<IDataProvider>(sp => sp.GetRequiredService<ClientDataProvider>())
                .AddScoped<ApplicationState>();
        }
    }
}