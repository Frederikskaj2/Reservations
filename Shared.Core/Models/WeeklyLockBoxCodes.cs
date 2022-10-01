using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared.Core;

public record WeeklyLockBoxCodes(int WeekNumber, LocalDate Date, IEnumerable<LockBoxCode> Codes);
