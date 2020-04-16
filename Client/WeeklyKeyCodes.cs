using System;
using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Client
{
    public class WeeklyKeyCodes
    {
        public WeeklyKeyCodes(int weekNumber, LocalDate date, IReadOnlyDictionary<int, string> codes)
        {
            WeekNumber = weekNumber;
            Date = date;
            Codes = codes ?? throw new ArgumentNullException(nameof(codes));
        }

        public int WeekNumber { get; }
        public LocalDate Date { get; }
        public IReadOnlyDictionary<int, string> Codes { get; }
    }
}