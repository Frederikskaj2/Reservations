using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

public record PaymentInformation(PaymentId PaymentId, Amount Amount, string AccountNumber);
