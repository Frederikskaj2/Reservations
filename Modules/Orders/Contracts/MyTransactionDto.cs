using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record MyTransactionDto(LocalDate Date, Activity Activity, OrderId? OrderId, string? Description, Amount Amount);
