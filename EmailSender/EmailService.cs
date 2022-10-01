using Frederikskaj2.Reservations.Shared.Core;
using Frederikskaj2.Reservations.Shared.Email;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.EmailSender;

class EmailService : BackgroundService
{
    readonly EmailApiService emailApiService;
    readonly ILogger logger;
    readonly MessageFactory messageFactory;
    readonly IOptionsMonitor<EmailSenderOptions> options;
    readonly EmailQueue queue;
    readonly AsyncRetryPolicy retryPolicy;

    public EmailService(
        EmailApiService emailApiService, ILogger<EmailService> logger, MessageFactory messageFactory, IOptionsMonitor<EmailSenderOptions> options,
        EmailQueue queue)
    {
        this.emailApiService = emailApiService;
        this.logger = logger;
        this.messageFactory = messageFactory;
        this.options = options;
        this.queue = queue;

        retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryForeverAsync(
                retryAttempt => TimeSpan.FromSeconds(Math.Min(Math.Pow(2, retryAttempt), 600)),
                (exception, _) =>
                {
                    logger.LogWarning(exception, "Cannot process email queue");
                });
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method should not pass exceptions to callers.")]
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!options.CurrentValue.IsEnabled)
        {
            logger.LogInformation("Email queue processing is disabled");
            return;
        }

        try
        {
            logger.LogDebug("Starting email queue processing");
            while (!cancellationToken.IsCancellationRequested)
            {
                await retryPolicy.ExecuteAsync(async () =>
                {
                    await foreach (var email in queue.DequeueAsync(cancellationToken))
                        await SendEmailAsync(email, cancellationToken);
                });
                await Task.Delay(options.CurrentValue.PollInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Email queue processing has unexpectedly stopped");
            return;
        }

        logger.LogDebug("Ending email queue processing");
    }

    async ValueTask SendEmailAsync(Email email, CancellationToken cancellationToken)
    {
        logger.LogDebug("Sending {Email}", email);
        var message = await messageFactory.CreateEmailAsync(email, cancellationToken);
        var (allowed, blocked) = FilterRecipients(options.CurrentValue.AllowedRecipients ?? Enumerable.Empty<string>(), message.To);
        if (allowed.Any())
        {
            await emailApiService.SendAsync(message with { To = allowed }, cancellationToken);
            logger.LogDebug("Sent email '{Subject}' to {Recipients}", message.Subject, allowed.Select(recipient => recipient.MaskEmail()));
        }
        if (blocked.Any())
            logger.LogDebug("Email '{Subject}' to {Recipients} was blocked", message.Subject, blocked.Select(recipient => recipient.MaskEmail()));
    }

    static (IEnumerable<string> AllowedRecipients, IEnumerable<string> BlockedRecipients) FilterRecipients(
        IEnumerable<string> allowedRecipients, IEnumerable<string> recipients) =>
    (
        recipients.Where(recipient => IsRecipientAllowed(allowedRecipients, recipient)),
        recipients.Where(recipient => !IsRecipientAllowed(allowedRecipients, recipient)));

    static bool IsRecipientAllowed(IEnumerable<string> allowedRecipients, string recipient) =>
        allowedRecipients.Count() is 0 ||
        allowedRecipients.Any(allowedRecipient => string.Equals(allowedRecipient, recipient, StringComparison.OrdinalIgnoreCase));
}
