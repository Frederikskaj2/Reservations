using Azure.Storage.Queues;
using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Emails;

class EmailQueue(IJobScheduler jobScheduler, ILogger<EmailQueue> logger, IOptionsMonitor<EmailQueueOptions> queueOptions) : IEmailEnqueuer, IEmailDequeuer
{
    const int maxMessages = 32;

    // Turn off distributed tracing to reduce data volume logged to application insights.
    static readonly QueueClientOptions queueClientOptions = new() { Diagnostics = { IsDistributedTracingEnabled = false } };

    public async ValueTask Enqueue(Email email, CancellationToken cancellationToken)
    {
        var options = queueOptions.CurrentValue;
        var client = await CreateQueueClient(options, cancellationToken);
        var json = JsonSerializer.Serialize(email, options.SerializerOptions);
        await client.SendMessageAsync(json, cancellationToken);
        logger.LogInformation("Queued email {Email} to {Recipient}", email.ToString(), email.ToEmail.Mask());
        jobScheduler.Queue(JobName.SendEmails);
    }

    public async IAsyncEnumerable<Email> Dequeue([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var options = queueOptions.CurrentValue;
        var client = await CreateQueueClient(options, cancellationToken);
        var response = await client.ReceiveMessagesAsync(maxMessages, cancellationToken: cancellationToken);
        var messages = response.Value.ToAsyncEnumerable().WithCancellation(cancellationToken);
        await foreach (var message in messages)
        {
            var email = await Deserialize(message.Body.ToStream());
            yield return email;
            await client.DeleteMessageAsync(message.MessageId, message.PopReceipt, cancellationToken);
        }

        ValueTask<Email> Deserialize(Stream stream) =>
            JsonSerializer.DeserializeAsync<Email>(stream, options.SerializerOptions, cancellationToken)!;
    }

    static async ValueTask<QueueClient> CreateQueueClient(EmailQueueOptions options, CancellationToken cancellationToken)
    {
        var client = new QueueClient(options.ConnectionString, options.QueueName, queueClientOptions);
        await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        return client;
    }
}
