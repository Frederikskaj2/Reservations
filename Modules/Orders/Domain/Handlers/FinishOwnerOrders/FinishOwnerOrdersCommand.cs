using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record FinishOwnerOrdersCommand(LocalDate Date);
