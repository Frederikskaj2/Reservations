using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

public record OrderConfirmedEmailModel(EmailAddress Email, string FullName, OrderId OrderId);