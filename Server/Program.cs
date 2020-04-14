using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Frederikskaj2.Reservations.Server
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            // Initialize the database.
            var scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
            using (var scope = scopeFactory.CreateScope())
            {
                var seedData = scope.ServiceProvider.GetRequiredService<SeedData>();
                await seedData.Initialize();
            }

            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(
                    webBuilder => webBuilder
                        .ConfigureAppConfiguration(builder => builder.AddJsonFile("appSettings.local.json", true, true))
                        .ConfigureLogging(
                            (context, builder) =>
                            {
                                var section = context.Configuration.GetSection("Logging");
                                builder.AddConfiguration(section);
                                builder.AddFile(section);
                            })
                        .UseStartup<Startup>());
    }
}