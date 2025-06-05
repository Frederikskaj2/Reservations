using NodaTime;

namespace Frederikskaj2.Reservations.Orders;

public record SendLockBoxCodesCommand(LocalDate Date);