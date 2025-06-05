using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Emails;

public class EmailSender(IEmailApiService emailApiService, ILogger<EmailSender> logger, IOptionsSnapshot<EmailSenderOptions> options)
{
    public async ValueTask Send(EmailMessage message, CancellationToken cancellationToken)
    {
        if (!options.Value.IsEnabled)
        {
            logger.LogDebug("Sending of email is disabled");
            return;
        }
        var (allowed, blocked) = FilterRecipients(options.Value.AllowedRecipients, message.To);
        if (allowed.Count > 0)
        {
            await emailApiService.Send(message with { To = allowed }, cancellationToken);
            logger.LogDebug("Sent email '{Subject}' to {Recipients}", message.Subject, allowed.Select(recipient => recipient.MaskEmail()));
        }
        if (blocked.Count > 0)
            logger.LogDebug("Email '{Subject}' to {Recipients} was blocked", message.Subject, blocked.Select(recipient => recipient.MaskEmail()));
    }

    static (IReadOnlyCollection<string> AllowedRecipients, IReadOnlyCollection<string> BlockedRecipients) FilterRecipients(
        IReadOnlySet<string> allowedRecipients, IReadOnlyCollection<string> recipients) =>
    (
        recipients.Where(recipient => IsRecipientAllowed(allowedRecipients, recipient)).ToList(),
        recipients.Where(recipient => !IsRecipientAllowed(allowedRecipients, recipient)).ToList());

    static bool IsRecipientAllowed(IReadOnlySet<string> allowedRecipients, string recipient) =>
        allowedRecipients.Count is 0 || allowedRecipients.Contains(recipient);
}
