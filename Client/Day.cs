using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Client
{
    public class Day
    {
        private static readonly HashSet<int> Empty = new HashSet<int>();

        public LocalDate Date { get; set; }
        public bool IsToday { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsHighPriceDay { get; set; }
        public bool IsResourceAvailable { get; set; }
        public HashSet<int> ReservedResourceIds { get; set; } = Empty;
    }
}