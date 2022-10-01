using LanguageExt;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Frederikskaj2.Reservations.Application;

public interface IEmailService
{
    Task<Unit> Send(CleaningScheduleEmail model, IEnumerable<EmailUser> users);
    Task<Unit> Send(ConfirmEmailEmailModel model);
    Task<Unit> Send(DebtReminderEmailModel model);
    Task<Unit> Send(LockBoxCodesEmail model);
    Task<Unit> Send(LockBoxCodesOverviewEmail model);
    Task<Unit> Send(NewOrderEmail model, IEnumerable<EmailUser> users);
    Task<Unit> Send(NewPasswordEmailModel model);
    Task<Unit> Send(NoFeeCancellationAllowedModel model);
    Task<Unit> Send(OrderConfirmedEmailModel model);
    Task<Unit> Send(OrderReceivedEmailModel model);
    Task<Unit> Send(PayInEmailModel model);
    Task<Unit> Send(PayOutEmailModel model);
    Task<Unit> Send(PostingsForMonthEmailModel model);
    Task<Unit> Send(ReservationsCancelledEmailModel model);
    Task<Unit> Send(ReservationSettledEmailModel model);
    Task<Unit> Send(SettlementNeededEmail model, IEnumerable<EmailUser> users);
    Task<Unit> Send(UserDeletedEmailModel model);
}
