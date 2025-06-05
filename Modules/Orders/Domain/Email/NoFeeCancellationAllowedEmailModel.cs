using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record NoFeeCancellationAllowedEmailModel(EmailAddress Email, string FullName, OrderId OrderId, Duration Duration);
