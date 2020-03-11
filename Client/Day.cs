using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Client
{
    public class Day
    {
        private static readonly IReadOnlyDictionary<int, bool> Empty = new Dictionary<int, bool>();

        public LocalDate Date { get; set; }
        public bool IsToday { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsHighPriceDay { get; set; }
        public bool IsResourceAvailable { get; set; }
        public IReadOnlyDictionary<int, bool> ReservedResources { get; set; } = Empty;
    }
}