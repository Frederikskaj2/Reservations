using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Entity Framework require an accessible setter.")]
    public class HistoryOrder
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public Instant CompletedTimestamp { get; set; }
        public List<Resource>? Resources { get; set; }
        public int Price { get; set; }
    }
}