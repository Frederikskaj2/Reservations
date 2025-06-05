using Frederikskaj2.Reservations.LockBox;
using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Calendar;

public record MyReservedDay(LocalDate Date, ResourceId ResourceId, OrderId OrderId, bool IsMyReservation);
