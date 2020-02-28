using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Blazor.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

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
            await builder.Build().RunAsync();
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // See https://github.com/dotnet/aspnetcore/issues/18733#issuecomment-585817720
            services.AddAuthorizationCore(options => { });
            services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
        }
    }
}