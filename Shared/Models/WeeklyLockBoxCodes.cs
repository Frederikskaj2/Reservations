using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class WeeklyLockBoxCodes
    {
        public int WeekNumber { get; set; }
        public LocalDate Date { get; set; }
        public IEnumerable<LockBoxCode>? Codes { get; set; }
    }
}