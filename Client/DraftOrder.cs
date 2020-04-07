using System.Collections.Generic;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Client
{
    public class DraftOrder
    {
        public PlaceOrderRequest Request { get; } = new PlaceOrderRequest();
        public List<DraftReservation> Reservations { get; } = new List<DraftReservation>();
        public DraftReservation? DraftReservation { get; private set; }

        public void AddReservation(Resource resource, LocalDate date, int durationInDays)
            => DraftReservation = new DraftReservation(resource, date, durationInDays);

        public void ClearDraftReservation() => DraftReservation = null;

        public void AddDraftReservationToOrder()
        {
            Reservations.Add(DraftReservation!);
            ClearDraftReservation();
        }

        public void RemoveReservation(DraftReservation reservation) => Reservations.Remove(reservation);

        public void Clear()
        {
            Request.Reservations.Clear();
            Reservations.Clear();
            DraftReservation = null;
        }

        public void PrepareRequest()
        {
            Request.Reservations.Clear();
            foreach (var reservation in Reservations)
            {
                var reservationRequest = new ReservationRequest
                {
                    ResourceId = reservation.Resource.Id,
                    Date = reservation.Date,
                    DurationInDays = reservation.DurationInDays
                };
                Request.Reservations.Add(reservationRequest);
            }
        }
    }
}