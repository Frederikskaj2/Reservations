using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record SendSettlementNeededRemindersCommand(LocalDate Date);
