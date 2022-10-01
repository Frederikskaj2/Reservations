using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

public record PayInEmailModel(EmailAddress Email, string FullName, Amount Amount, PaymentInformation? Payment);
