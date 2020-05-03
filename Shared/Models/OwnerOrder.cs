using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class OwnerOrder
    {
        public int Id { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public IEnumerable<Reservation>? Reservations { get; set; }
        public bool IsCleaningRequired { get; set; }
        public string? CreatedByEmail { get; set; }
        public string? CreatedByName { get; set; }
    }
}