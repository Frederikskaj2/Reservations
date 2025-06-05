using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Emails;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using static Frederikskaj2.Reservations.Users.PaymentIdEncoder;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Bank;

class BankEmailService(IOptionsSnapshot<EmailsOptions> options, IEmailEnqueuer emailEnqueuer) : IBankEmailService
{
    readonly EmailsOptions options = options.Value;

    public async Task<Unit> Send(DebtReminderEmailModel model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, paymentInformation) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl) { DebtReminder = new(paymentInformation) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(PayInEmailModel model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, amount, payment) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl) { PayIn = new(amount, payment.ToNullableReference()) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(PayOutEmailModel model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, amount) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl) { PayOut = new(amount) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(PostingsForMonthEmailModel model, HashMap<UserId, string> userFullNames, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, month, postings) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl)
        {
            PostingsForMonth = new(
                month,
                AccountNames.All,
                postings.Map(
                    posting => new PostingDto(
                        posting.TransactionId,
                        posting.Date,
                        posting.Activity,
                        posting.ResidentId,
                        userFullNames[posting.ResidentId],
                        FromUserId(posting.ResidentId),
                        posting.OrderId.ToNullable(),
                        posting.Amounts.ToSeq().Map(tuple => new AccountAmountDto(tuple.Key, tuple.Value))))),
        };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }
}
