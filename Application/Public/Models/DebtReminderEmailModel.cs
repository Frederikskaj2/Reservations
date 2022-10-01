using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

public record DebtReminderEmailModel(EmailAddress Email, string FullName, PaymentInformation? Payment);
