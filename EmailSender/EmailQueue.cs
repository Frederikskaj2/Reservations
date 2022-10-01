using Azure.Storage.Queues;
using Frederikskaj2.Reservations.Shared.Email;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.EmailSender;

class EmailQueue
{
    readonly IOptionsMonitor<EmailQueueOptions> queueOptions;

    public EmailQueue(IOptionsMonitor<EmailQueueOptions> queueOptions) => this.queueOptions = queueOptions;

    public async IAsyncEnumerable<Email> DequeueAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var options = queueOptions.CurrentValue;
        var client = await CreateQueueClientAsync(options, cancellationToken);
        var response = await client.ReceiveMessagesAsync(32, cancellationToken: cancellationToken);
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

    static async ValueTask<QueueClient> CreateQueueClientAsync(EmailQueueOptions options, CancellationToken cancellationToken)
    {
        var client = new QueueClient(options.ConnectionString, options.QueueName);
        await client.CreateIfNotExistsAsync(cancellationToken: cancellationToken);
        return client;
    }
}
