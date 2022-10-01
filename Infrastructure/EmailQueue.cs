using Azure.Storage.Queues;
using Frederikskaj2.Reservations.Application;
using Frederikskaj2.Reservations.Shared.Email;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Infrastructure;

sealed class EmailQueue : IEmailQueue
{
    readonly IOptionsMonitor<EmailQueueOptions> queueOptions;

    public EmailQueue(IOptionsMonitor<EmailQueueOptions> queueOptions) => this.queueOptions = queueOptions;

    public async ValueTask Enqueue(Email email)
    {
        var options = queueOptions.CurrentValue;
        var client = await CreateClientAsync(options);
        var json = JsonSerializer.Serialize(email, options.SerializerOptions);
        await client.SendMessageAsync(json);
    }

    static async ValueTask<QueueClient> CreateClientAsync(EmailQueueOptions options)
    {
        var client = new QueueClient(options.ConnectionString, options.QueueName);
        await client.CreateIfNotExistsAsync();
        return client;
    }
}
