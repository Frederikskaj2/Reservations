using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        var hostEnvironment = host.Services.GetRequiredService<IHostEnvironment>();
        if (IsDevelopmentEnvironment(hostEnvironment))
            await InitializeDatabase(host);

        await host.RunAsync();
    }

    static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(
                webBuilder => webBuilder
                    .ConfigureAppConfiguration((context, builder) =>
                    {
                        if (IsDevelopmentEnvironment(context.HostingEnvironment))
                            builder.AddJsonFile("appsettings.local.json", true, true);
                    })
                    .UseStartup<Startup>());

    static async Task InitializeDatabase(IHost host)
    {
        var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
        using var scope = scopeFactory.CreateScope();
        var seedData = scope.ServiceProvider.GetRequiredService<SeedData>();
        await seedData.InitializeAsync();
    }

    static bool IsDevelopmentEnvironment(IHostEnvironment hostEnvironment) =>
        hostEnvironment.EnvironmentName is not "Production" and not "Test";
}
