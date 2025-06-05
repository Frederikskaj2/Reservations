using Azure.Storage.Queues;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Emails;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Persistence;
using LanguageExt;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using Serilog;
using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

class ApplicationFactory(string id) : WebApplicationFactory<Program>
{
    CosmosOptions? cosmosOptions;
    EmailQueueOptions? emailQueueOptions;
    QueueClient? queueClient;

    public JsonSerializerOptions JsonSerializerOptions => cosmosOptions?.SerializerOptions ?? throw new InvalidOperationException();

    public QueueClient QueueClient => queueClient ??= CreateQueueClient();

    public LocalDate TestStartDate { get; private set; }

    public Period NowOffset { get; set; } = Period.Zero;

    public LocalDate CurrentDate => TestStartDate.Plus(NowOffset);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("IntegrationTest");
        CollectionFormat.MaxShortItems = 10;
        var logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(
                $@"Logs\{DateTime.Now:yyyyMMddHHmmss}-{id}.log",
                outputTemplate: "{Timestamp:HH:mm:ss.fff} [{Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}",
                formatProvider: CultureInfo.InvariantCulture)
            .CreateLogger();
        builder
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(logger, dispose: true);
            })
            .ConfigureServices(
                services =>
                {
                    services.AddSingleton<IConfigureOptions<CosmosOptions>>(new ConfigureCosmosOptions($"Test{id}"));
                    services.AddSingleton<IConfigureOptions<DateProviderOptions>>(new ConfigureDateProviderOptions(this));
                    services.AddSingleton<IConfigureOptions<EmailQueueOptions>>(new ConfigureEmailQueueOptions($"email{id}"));
                    services.AddSingleton<IConfigureOptions<EmailSenderOptions>>(new ConfigureEmailSenderOptions());
                    services.AddSingleton<IConfigureOptions<JobSchedulerOptions>>(new ConfigureJobSchedulerOptions());
                    services.AddSingleton<IConfigureOptions<OrderingOptions>>(new ConfigureOrderingOptions());
                });
    }

    public async ValueTask Initialize()
    {
        await using var scope = Server.Services.CreateAsyncScope();

        cosmosOptions = scope.ServiceProvider.GetRequiredService<IOptions<CosmosOptions>>().Value;
        emailQueueOptions = scope.ServiceProvider.GetRequiredService<IOptions<EmailQueueOptions>>().Value;
        var dateProvider = scope.ServiceProvider.GetRequiredService<IDateProvider>();
        TestStartDate = dateProvider.Today;

        var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await SeedData.CreateAdministrator(databaseInitializer);
    }

    public override async ValueTask DisposeAsync()
    {
        if (cosmosOptions is not null)
        {
            using var client = new CosmosClient(cosmosOptions.ConnectionString);
            var container = client.GetContainer(cosmosOptions.DatabaseId, cosmosOptions.ContainerId);
            await container.DeleteContainerStreamAsync();

            await QueueClient.DeleteIfExistsAsync();
        }
        await base.DisposeAsync();
    }

    QueueClient CreateQueueClient() =>
        new(emailQueueOptions?.ConnectionString ?? throw new InvalidOperationException(), emailQueueOptions.QueueName);

    class ConfigureCosmosOptions(string containerId) : IConfigureOptions<CosmosOptions>
    {
        public void Configure(CosmosOptions options)
        {
            options.DatabaseId = "Frederikskaj2";
            options.ContainerId = containerId;
        }
    }

    class ConfigureDateProviderOptions(ApplicationFactory factory) : IConfigureOptions<DateProviderOptions>
    {
        public void Configure(DateProviderOptions options) => options.NowOffset = factory.NowOffset;
    }

    class ConfigureEmailQueueOptions(string queueId) : IConfigureOptions<EmailQueueOptions>
    {
        public void Configure(EmailQueueOptions options) => options.QueueName = queueId;
    }

    class ConfigureEmailSenderOptions : IConfigureOptions<EmailSenderOptions>
    {
        public void Configure(EmailSenderOptions options) => options.IsEnabled = false;
    }

    class ConfigureJobSchedulerOptions : IConfigureOptions<JobSchedulerOptions>
    {
        public void Configure(JobSchedulerOptions options) => options.IsEnabled = false;
    }

    class ConfigureOrderingOptions : IConfigureOptions<OrderingOptions>
    {
        public void Configure(OrderingOptions options)
        {
            options.Testing ??= new();
            options.Testing.IsTestingEnabled = true;
            options.Testing.IsSettlementAlwaysAllowed = true;
        }
    }
}
