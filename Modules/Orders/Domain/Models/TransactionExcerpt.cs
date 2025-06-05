using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record TransactionExcerpt(TransactionId TransactionId, UserId AdministratorId, Instant Timestamp, UserId ResidentId);
