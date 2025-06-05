using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

public record DebtReminderEmailModel(EmailAddress Email, string FullName, PaymentInformation Payment);
