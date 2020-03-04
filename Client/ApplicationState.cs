using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Client
{
    public class ApplicationState
    {
        public string? RedirectUrl { get; set; }
        public Reservation Reservation { get; set; } = new Reservation { Reservations = new List<ResourceReservation>() };
    }
}