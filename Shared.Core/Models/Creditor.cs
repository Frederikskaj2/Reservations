namespace Frederikskaj2.Reservations.Shared.Core;

public record Creditor(UserInformation UserInformation, PaymentId PaymentId, string AccountNumber, Amount Amount);
