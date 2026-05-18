using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Emails;

public record SettlementNeededDto(
    OrderId OrderId,
    ResourceId ResourceId,
    LocalDate Date);
