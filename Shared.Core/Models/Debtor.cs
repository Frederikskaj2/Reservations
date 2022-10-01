namespace Frederikskaj2.Reservations.Shared.Core;

public record Debtor(PaymentId PaymentId, UserInformation UserInformation, Amount Amount);
