using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Emails;
using Frederikskaj2.Reservations.Users;
using LanguageExt;
using Microsoft.Extensions.Options;
using System.Threading;
using System.Threading.Tasks;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Orders;

class OrdersEmailService(IOptionsSnapshot<EmailsOptions> options, IEmailEnqueuer emailEnqueuer) : IOrdersEmailService
{
    readonly EmailsOptions options = options.Value;

    public async Task<Unit> Send(LockBoxCodesEmail model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, orderId, resourceId, date, codes) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl) { LockBoxCodes = new(orderId, resourceId, date, codes) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(NewOrderEmailModel model, Seq<EmailUser> users, CancellationToken cancellationToken)
    {
        var orderId = model.OrderId;
        foreach (var user in users)
        {
            var email = new Email(user.Email, user.FullName, options.BaseUrl) { NewOrder = new(orderId) };
            await emailEnqueuer.Enqueue(email, cancellationToken);
        }
        return unit;
    }

    public async Task<Unit> Send(NoFeeCancellationAllowedEmailModel model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, orderId, duration) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl) { NoFeeCancellationAllowed = new(orderId, duration) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(OrderConfirmedEmailModel model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, orderId) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl) { OrderConfirmed = new(orderId) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(OrderReceivedEmailModel model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, orderId, paymentInformation) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl) { OrderReceived = new(orderId, paymentInformation.ToNullableReference()) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(ReservationsCancelledEmailModel model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, orderId, reservations, refund, fee) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl) { ReservationsCancelled = new(orderId, reservations, refund, fee) };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(ReservationSettledEmailModel model, CancellationToken cancellationToken)
    {
        var (emailAddress, fullName, orderId, reservation, deposit, damages, description) = model;
        var email = new Email(emailAddress, fullName, options.BaseUrl)
        {
            ReservationSettled = new(orderId, reservation, deposit, damages, description),
        };
        await emailEnqueuer.Enqueue(email, cancellationToken);
        return unit;
    }

    public async Task<Unit> Send(SettlementNeededEmailModel model, Seq<EmailUser> users, CancellationToken cancellationToken)
    {
        var (orderId, resourceId, date) = model;
        foreach (var user in users)
        {
            var email = new Email(user.Email, user.FullName, options.BaseUrl) { SettlementNeeded = new(orderId, resourceId, date) };
            await emailEnqueuer.Enqueue(email, cancellationToken);
        }
        return unit;
    }
}
