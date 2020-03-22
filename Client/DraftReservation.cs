using System;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Client
{
    public class DraftReservation
    {
        public DraftReservation(Resource resource, LocalDate date, int durationInDays)
        {
            Resource = resource ?? throw new ArgumentNullException(nameof(resource));
            Date = date;
            DurationInDays = durationInDays;
        }

        public Resource Resource { get; }
        public LocalDate Date { get; }
        public int DurationInDays { get; set; }
        public Price? Price { get; set; }
    }
}