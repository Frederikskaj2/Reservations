using Frederikskaj2.Reservations.Users;
using LanguageExt;
using System.Threading;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Orders;

public interface IOrdersEmailService
{
    Task<Unit> Send(LockBoxCodesEmail model, CancellationToken cancellationToken);
    Task<Unit> Send(NewOrderEmailModel model, Seq<EmailUser> users, CancellationToken cancellationToken);
    Task<Unit> Send(NoFeeCancellationAllowedEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(OrderConfirmedEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(OrderReceivedEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(ReservationsCancelledEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(ReservationSettledEmailModel model, CancellationToken cancellationToken);
    Task<Unit> Send(SettlementNeededEmailModel model, Seq<EmailUser> users, CancellationToken cancellationToken);
}
