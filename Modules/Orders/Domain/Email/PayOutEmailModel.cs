using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Orders;

public record PayOutEmailModel(EmailAddress Email, string FullName, Amount Amount);