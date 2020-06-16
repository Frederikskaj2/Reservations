using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;
using Frederikskaj2.Reservations.Server.Data;

namespace Frederikskaj2.Reservations.Server.Email
{
    public interface IEmailService
    {
        Task SendCleaningScheduleEmail(User user, IEnumerable<Data.Resource> resources, IEnumerable<Data.CleaningTask> cancelledTasks, IEnumerable<Data.CleaningTask> newTasks, IEnumerable<Data.CleaningTask> currentTasks);
        Task SendConfirmEmail(User user, string token);
        Task SendKeyCodeEmail(User user, Reservation reservation, IEnumerable<KeyCode> keyCodes);
        Task SendKeyCodesEmail(User user, IEnumerable<Data.Resource> resources, IEnumerable<KeyCode> keyCodes);
        Task SendNewOrderEmail(User user, int orderId);
        Task SendOrderConfirmedEmail(User user, int orderId);
        Task SendOrderReceivedEmail(User user, int orderId, int prepaidAmount, int amount);
        Task SendOverduePaymentEmail(User user, int orderId);
        Task SendPayInEmail(User user, int orderId, int amount, int missingAmount);
        Task SendPayOutEmail(User user, int amount);
        Task SendPostingsEmail(User user, IEnumerable<Shared.Posting> postings);
        Task SendReservationCancelledEmail(User user, int orderId, string resourceName, LocalDate date, int total, int cancellationFee);
        Task SendReservationSettledEmail(User user, int orderId, string resourceName, LocalDate date, int deposit, int damages, string? damagesDescription);
        Task SendResetPasswordEmail(User user, string token);
        Task SendSettlementNeededEmail(User user, int orderId, string resourceName, LocalDate date);
        Task SendUserDeletedEmail(User user);
    }
}