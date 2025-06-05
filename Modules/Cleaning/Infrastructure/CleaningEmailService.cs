using Frederikskaj2.Reservations.Emails;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Cleaning;

class CleaningEmailService(IOptionsSnapshot<EmailsOptions> options, IEmailEnqueuer emailEnqueuer) : ICleaningEmailService
{
    readonly EmailsOptions options = options.Value;

    public async Task<Unit> Send(CleaningScheduleEmail model, Seq<EmailUser> users, CancellationToken cancellationToken)
    {
        var (schedule, delta) = model;
        foreach (var user in users)
        {
            var email = new Email(user.Email, user.FullName, options.BaseUrl)
            {
                CleaningScheduleOverview = CleaningScheduleOverViewFactory.Create(schedule, delta),
            };
            await emailEnqueuer.Enqueue(email, cancellationToken);
        }
        return unit;
    }
}
