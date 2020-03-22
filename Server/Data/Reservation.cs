using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class Reservation
    {
        public int Id { get; set; }
        public Order? Order { get; set; }
        public int ResourceId { get; set; }
        public Resource? Resource { get; set; }
        public ReservationStatus Status { get; set; }
        public Instant UpdatedTimestamp { get; set; }
        public List<ReservedDay>? Days { get; set; }
        public Price? Price { get; set; }
    }
}