using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Calendar;

public record GetReservedDaysResponse(IEnumerable<ReservedDayDto> ReservedDays);
