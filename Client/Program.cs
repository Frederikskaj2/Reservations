using Blazored.LocalStorage;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using Frederikskaj2.Reservations.Bank;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Frederikskaj2.Reservations.Client;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

const string apiHttpClientName = "Api";

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Logging.SetMinimumLevel(LogLevel.Trace);
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

builder.Services
    .AddBlazorise(options => options.Immediate = true)
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

ConfigureServices(builder.Services, new(builder.HostEnvironment.BaseAddress));

await builder.Build().RunAsync();

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
        .AddSingleton<IJsonTypeInfoResolver, DefaultJsonTypeInfoResolver>()
        .AddBankSharedDomain()
        .AddCoreSharedDomain()
        .AddLockBoxSharedDomain()
        .AddOrdersSharedDomain()
        .AddUsersSharedDomain()
        .AddOptions<JsonSerializerOptions>();

    services
        .AddSingleton(serviceProvider => serviceProvider.GetRequiredService<IHttpClientFactory>().CreateClient(apiHttpClientName))
        .AddSingleton<ApiClient>()
        .AddSingleton<AuthenticatedApiClient>();

    services
        .AddBlazoredLocalStorageAsSingleton()
        .AddAuthorizationCore(options =>
        {
            options.AddPolicy(
                Policy.ViewOrders,
                policy => policy.RequireRole(nameof(Roles.OrderHandling), nameof(Roles.Bookkeeping), nameof(Roles.UserAdministration)));
            options.AddPolicy(
                Policy.ViewUsers,
                policy => policy.RequireRole(nameof(Roles.OrderHandling), nameof(Roles.Bookkeeping), nameof(Roles.UserAdministration)));
        })
        .AddSingleton<AuthenticationService>()
        .AddSingleton<TokenAuthenticationStateProvider>()
        .AddSingleton<AuthenticationStateProvider>(serviceProvider => serviceProvider.GetRequiredService<TokenAuthenticationStateProvider>());

    services
        .AddSingleton<IDateProvider, DateProvider>()
        .AddCoreSharedInfrastructure();

    services
        .AddSingleton<DraftOrder>()
        .AddSingleton<EventAggregator>()
        .AddSingleton<UserOrderInformation>()
        .AddSingleton<ClientDataProvider>()
        .AddSingleton<Formatter>()
        .AddSingleton<RedirectState>()
        .AddSingleton<SignOutService>()
        .AddSingleton<SignUpState>()
        .AddSingleton<SignInState>();
}
