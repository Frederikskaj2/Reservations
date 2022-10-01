using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Shared.Email;

public record DebtReminder(EmailAddress Email, string FullName, PaymentInformation? Payment) : MessageBase(Email, FullName);
