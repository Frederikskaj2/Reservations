namespace Frederikskaj2.Reservations.Shared.Core;

public record PaymentInformation(PaymentId PaymentId, Amount Amount, string AccountNumber);
