using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record RemoveAccountNumbersCommand(Instant Timestamp, UserId AdministratorId);
