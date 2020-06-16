using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server;
using Frederikskaj2.Reservations.Server.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Frederikskaj2.Reservations.Tests
{
    [SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "This implementation is used to integrate with xUnit.")]
    public abstract class IntegrationTests : IDisposable
    {
        private static readonly Random random = new Random();
        private string fileName;
        private IServiceScopeFactory scopeFactory;

        protected IntegrationTests()
        {
        }

        protected Task Initialize(Action<IServiceCollection, IConfiguration> configureServices = null)
        {
            fileName = $"{random.Next():x8}.db";
            var host = new HostBuilder()
                .ConfigureHostConfiguration(builder => builder.AddJsonFile("appsettings.json"))
                .ConfigureServices((context, services) =>
                {
                    var startup = new Startup(context.Configuration);
                    startup.ConfigureServicesWithConnectionString(services, $"Filename={fileName}");
                    configureServices?.Invoke(services, context.Configuration);
                })
                .Build();

            scopeFactory = host.Services.GetRequiredService<IServiceScopeFactory>();
            using var scope = scopeFactory.CreateScope();
            var seedData = scope.ServiceProvider.GetRequiredService<SeedData>();
            return seedData.Initialize();
        }

        protected IServiceScope CreateScope() => scopeFactory.CreateScope();

        protected async Task ModifyDatabase(Action<ReservationsContext> modifyAction)
        {
            if (modifyAction is null)
                throw new ArgumentNullException(nameof(modifyAction));

            using var scope = CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ReservationsContext>();
            modifyAction(db);
            await db.SaveChangesAsync();
        }

        [SuppressMessage("Usage", "CA1816:Dispose methods should call SuppressFinalize", Justification = "This implementation is used to integrate with xUnit.")]
        public virtual void Dispose() => File.Delete(fileName);
    }
}
