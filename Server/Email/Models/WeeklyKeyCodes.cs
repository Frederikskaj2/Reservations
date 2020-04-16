using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class WeeklyKeyCodes
    {
        public WeeklyKeyCodes(int weekNumber, LocalDate date, IReadOnlyDictionary<int, string> codes)
        {
            WeekNumber = weekNumber;
            Date = date;
            Codes = codes;
        }

        public int WeekNumber { get; }
        public LocalDate Date { get; }
        public IReadOnlyDictionary<int, string> Codes { get; }
    }
}
