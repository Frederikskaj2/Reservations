using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record PayInEmailModel(EmailAddress Email, string FullName, Amount Amount, Option<PaymentInformation> Payment);
