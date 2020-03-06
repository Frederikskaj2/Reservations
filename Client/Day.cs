using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Client
{
    public class Day
    {
        private static readonly Dictionary<int, Reservation> Empty = new Dictionary<int, Reservation>();

        public LocalDate Date { get; set; }
        public bool IsToday { get; set; }
        public bool IsCurrentMonth { get; set; }
        public bool IsHighPriceDay { get; set; }
        public bool IsResourceAvailable { get; set; }
        public IReadOnlyDictionary<int, Reservation> Reservations { get; set; } = Empty;
    }
}