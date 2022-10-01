using Azure.Storage.Queues;
using Frederikskaj2.Reservations.Application.BackgroundServices;
using Frederikskaj2.Reservations.EmailSender;
using Frederikskaj2.Reservations.Infrastructure.Persistence;
using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Email;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Harness;

class ApplicationFactory : WebApplicationFactory<Startup>
{
    readonly string id;
    CosmosOptions? cosmosOptions;
    EmailQueueOptions? emailQueueOptions;
    QueueClient? queueClient;

    public ApplicationFactory(string id) => this.id = id;

    public JsonSerializerOptions JsonSerializerOptions => cosmosOptions?.SerializerOptions ?? throw new InvalidOperationException();

    public QueueClient QueueClient => queueClient ??= CreateQueueClient();

    public LocalDate TestStartDate { get; private set; }

    public Period NowOffset { get; set; } = Period.Zero;

    public LocalDate CurrentDate => TestStartDate.Plus(NowOffset);

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder
            .ConfigureLogging(loggingBuilder => loggingBuilder.AddFile(options =>
            {
                options.FileName = $"{id}-";
                options.Extension = "log";
                options.FlushPeriod = TimeSpan.FromTicks(1);
            }))
            .ConfigureServices(services =>
            {
                services.AddSingleton<IConfigureOptions<CosmosOptions>>(new ConfigureCosmosOptions($"Test{id}"));
                services.AddSingleton<IConfigureOptions<DateProviderOptions>>(new ConfigureDateProviderOptions(this));
                services.AddSingleton<IConfigureOptions<EmailQueueOptions>>(new ConfigureEmailQueueOptions($"email{id}"));
                services.AddSingleton<IConfigureOptions<EmailSenderOptions>>(new ConfigureEmailSenderOptions());
                services.AddSingleton<IConfigureOptions<ScheduledEmailOptions>>(new ConfigureScheduledEmailOptions());
                services.AddSingleton<IConfigureOptions<TestingOptions>>(new ConfigureTestingOptions());
            });

    public async ValueTask InitializeAsync()
    {
        using var scope = Server.Services.CreateScope();

        cosmosOptions = scope.ServiceProvider.GetRequiredService<IOptions<CosmosOptions>>().Value;
        emailQueueOptions = scope.ServiceProvider.GetRequiredService<IOptions<EmailQueueOptions>>().Value;
        var dateProvider = scope.ServiceProvider.GetRequiredService<IDateProvider>();
        TestStartDate = dateProvider.Today;

        var databaseInitializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
        await TestData.CreateAdministratorAsync(databaseInitializer);
    }

    public override async ValueTask DisposeAsync()
    {
        if (cosmosOptions is not null)
        {
            using var client = new CosmosClient(cosmosOptions.ConnectionString);
            var container = client.GetContainer(cosmosOptions.DatabaseId, cosmosOptions.ContainerId);
            await container.DeleteContainerStreamAsync();

            var queueClient = new QueueClient(emailQueueOptions!.ConnectionString, emailQueueOptions.QueueName);
            await queueClient.DeleteIfExistsAsync();
        }
        await base.DisposeAsync();
    }

    QueueClient CreateQueueClient() =>
        new(emailQueueOptions?.ConnectionString ?? throw new InvalidOperationException(), emailQueueOptions.QueueName);

    class ConfigureCosmosOptions : IConfigureOptions<CosmosOptions>
    {
        readonly string containerId;

        public ConfigureCosmosOptions(string containerId) => this.containerId = containerId;

        public void Configure(CosmosOptions options)
        {
            options.DatabaseId = "Frederikskaj2";
            options.ContainerId = containerId;
        }
    }

    class ConfigureDateProviderOptions : IConfigureOptions<DateProviderOptions>
    {
        readonly ApplicationFactory factory;

        public ConfigureDateProviderOptions(ApplicationFactory factory) => this.factory = factory;

        public void Configure(DateProviderOptions options) => options.NowOffset = factory.NowOffset;
    }

    class ConfigureEmailQueueOptions : IConfigureOptions<EmailQueueOptions>
    {
        readonly string queueId;

        public ConfigureEmailQueueOptions(string queueId) => this.queueId = queueId;

        public void Configure(EmailQueueOptions options) => options.QueueName = queueId;
    }

    class ConfigureEmailSenderOptions : IConfigureOptions<EmailSenderOptions>
    {
        public void Configure(EmailSenderOptions options) => options.IsEnabled = false;
    }

    class ConfigureScheduledEmailOptions : IConfigureOptions<ScheduledEmailOptions>
    {
        public void Configure(ScheduledEmailOptions options) => options.IsEnabled = false;
    }

    class ConfigureTestingOptions : IConfigureOptions<TestingOptions>
    {
        public void Configure(TestingOptions options)
        {
            options.IsTestingEnabled = true;
            options.IsSettlementAlwaysAllowed = true;
        }
    }
}
