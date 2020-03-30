using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class HistoryOrder
    {
        public int Id { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public Instant CompletedTimestamp { get; set; }
        public List<Resource>? Resources { get; set; }
        public int Price { get; set; }
    }
}