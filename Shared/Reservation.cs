using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Shared
{
    public class Reservation
    {
        public int Id { get; set; }
        public User? User { get; set; }
        public Apartment? Apartment { get; set; }
        public DateTime CreatedTimestamp { get; set; }
        public DateTime UpdatedTimestamp { get; set; }
        public List<ResourceReservation>? Reservations { get; set; }
    }
}