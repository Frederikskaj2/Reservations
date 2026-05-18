using Frederikskaj2.Reservations.Emails;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.RoomAccess;

class RoomAccessEmailService(IOptionsSnapshot<EmailsOptions> options, IEmailEnqueuer emailEnqueuer) : IRoomAccessEmailService
{
    readonly EmailsOptions options = options.Value;

    public async Task<Unit> Send(RoomEntryCodeEmail model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, orderId, resourceId, date, code) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl) { RoomEntryCode = new(orderId, resourceId, date, code) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }
}
