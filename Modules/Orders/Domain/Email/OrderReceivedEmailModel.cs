using Frederikskaj2.Reservations.Users;
using LanguageExt;

namespace Frederikskaj2.Reservations.Orders;

public record OrderReceivedEmailModel(EmailAddress Email, string FullName, OrderId OrderId, Option<PaymentInformation> Payment);
