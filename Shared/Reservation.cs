using System;
using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public class Reservation
    {
        public int Id { get; set; }
        public User? User { get; set; }
        public Apartment? Apartment { get; set; }
        public Instant CreatedTimestamp { get; set; }
        public Instant UpdatedTimestamp { get; set; }
        public List<ResourceReservation>? Reservations { get; set; }
    }
}