using Frederikskaj2.Reservations.Emails;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.LockBox;

class LockBoxEmailService(IOptionsSnapshot<EmailsOptions> options, IEmailEnqueuer emailEnqueuer) : ILockBoxEmailService
{
    readonly EmailsOptions options = options.Value;

    public async Task<Unit> Send(LockBoxCodesOverviewEmail model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, codes) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl)
        {
            LockBoxCodesOverview = new(
                Resources.All,
                codes.Map(
                    weeklyLockBoxCodes => new WeeklyLockBoxCodesDto(
                        weeklyLockBoxCodes.WeekNumber,
                        weeklyLockBoxCodes.Date,
                        weeklyLockBoxCodes.Codes.Map(weeklyLockBoxCode => new WeeklyLockBoxCodeDto(
                            weeklyLockBoxCode.ResourceId,
                            weeklyLockBoxCode.Code,
                            weeklyLockBoxCode.Difference))))),
        };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }
}
