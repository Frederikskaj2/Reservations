using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Client;

public static class Program
{
    const string apiHttpClientName = "Api";

    public static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.Logging.SetMinimumLevel(LogLevel.Trace);
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

        builder.RootComponents.Add<HeadOutlet>("head::after");;
        builder.RootComponents.Add<App>("#app");

        builder.Services
            .AddBlazorise(options => options.Immediate = true)
            .AddBootstrapProviders()
            .AddFontAwesomeIcons();

        ConfigureServices(builder.Services, new Uri(builder.HostEnvironment.BaseAddress));

        var host = builder.Build();

        await host.RunAsync();
    }

    static void ConfigureServices(IServiceCollection services, Uri baseAddress)
    {
        services
            .AddHttpClient(apiHttpClientName, client => client.BaseAddress = baseAddress);

        services
            .AddSingleton(serviceProvider =>
            {
                var configureOptions = serviceProvider.GetRequiredService<IConfigureOptions<JsonSerializerOptions>>();
                var options = new JsonSerializerOptions();
                configureOptions.Configure(options);
                return options;
            })
            .AddSerialization()
            .AddOptions<JsonSerializerOptions>();

        services
            .AddScoped(serviceProvider => serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(apiHttpClientName))
            .AddScoped<ApiClient>()
            .AddScoped<AuthenticatedApiClient>();

        services
            .AddBlazoredLocalStorage()
            .AddAuthorizationCore(options =>
            {
                options.AddPolicy(
                    Policies.ViewOrders,
                    policy => policy.RequireRole(nameof(Roles.OrderHandling), nameof(Roles.Bookkeeping), nameof(Roles.UserAdministration)));
                options.AddPolicy(
                    Policies.ViewUsers,
                    policy => policy.RequireRole(nameof(Roles.OrderHandling), nameof(Roles.Bookkeeping), nameof(Roles.UserAdministration)));
            })
            .AddScoped<AuthenticationService>()
            .AddScoped<TokenAuthenticationStateProvider>()
            .AddScoped<AuthenticationStateProvider>(serviceProvider => serviceProvider.GetRequiredService<TokenAuthenticationStateProvider>());

        services
            .AddBlazorise(options => options.Immediate = true)
            .AddBootstrapProviders()
            .AddFontAwesomeIcons();

        services
            .AddShared();

        services
            .AddSingleton<AsyncEventAggregator>()
            .AddSingleton<DraftOrder>()
            .AddSingleton<EventAggregator>()
            .AddSingleton<UserOrderInformation>()
            .AddScoped<ClientDataProvider>()
            .AddScoped<Formatter>()
            .AddScoped<RedirectState>()
            .AddScoped<SignOutService>()
            .AddScoped<SignUpState>()
            .AddScoped<SignInState>();
    }
}
