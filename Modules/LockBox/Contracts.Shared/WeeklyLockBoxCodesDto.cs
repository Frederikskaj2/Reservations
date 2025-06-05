using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.LockBox;

public record WeeklyLockBoxCodesDto(int WeekNumber, LocalDate Date, IEnumerable<WeeklyLockBoxCodeDto> Codes);