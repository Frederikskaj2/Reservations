using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;

namespace Frederikskaj2.Reservations.Client
{
    [SuppressMessage(
        "Design", "RCS1102:Make class static.",
        Justification = "Blazor client side app cannot be hosted in ASP.NET Core website if it is static.")]
    [SuppressMessage(
        "Design", "CA1052:Static holder types should be Static or NotInheritable",
        Justification = "Blazor client side app cannot be hosted in ASP.NET Core website if it is static.")]
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");
            ConfigureServices(builder.Services, builder.HostEnvironment);
            var host = builder.Build();
            host.Services
                .UseBootstrapProviders()
                .UseFontAwesomeIcons();
            await host.RunAsync();
        }

        [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "The HttpClient should never be disposed.")]
        private static void ConfigureServices(IServiceCollection services, IWebAssemblyHostEnvironment hostEnvironment)
        {
            var httpClient = new HttpClient { BaseAddress = new Uri(hostEnvironment.BaseAddress) };
            services
                .AddSingleton(httpClient);

            services
                .AddBlazorise(options => options.ChangeTextOnKeyPress = true)
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

            var cultureInfo = new CultureInfo("da-DK")
            {
                NumberFormat =
                {
                    CurrencyPositivePattern = 2,
                    CurrencyNegativePattern = 12
                }
            };
            services
                .AddSingleton(cultureInfo)
                .AddSingleton<FormattingService>()
                .AddReservationsServices()
                .AddSingleton<ClientDataProvider>()
                .AddSingleton<IDataProvider>(sp => sp.GetRequiredService<ClientDataProvider>())
                .AddScoped<ApplicationState>();
        }
    }
}