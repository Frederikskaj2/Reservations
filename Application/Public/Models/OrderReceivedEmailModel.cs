using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

public record OrderReceivedEmailModel(EmailAddress Email, string FullName, OrderId OrderId, PaymentInformation? Payment);
