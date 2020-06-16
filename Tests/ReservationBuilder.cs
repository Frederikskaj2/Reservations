using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Shared;
using NodaTime;
using Reservation = Frederikskaj2.Reservations.Server.Data.Reservation;

namespace Frederikskaj2.Reservations.Tests
{
    internal sealed class ReservationBuilder
    {
        private readonly Reservation reservation = new Reservation();

        private ReservationBuilder()
        {
        }

        public static ReservationBuilder CreateReservation() => new ReservationBuilder();

        public ReservationBuilder WithResourceId(int resourceId)
        {
            reservation.ResourceId = resourceId;
            return this;
        }

        public ReservationBuilder WithStatus(ReservationStatus status)
        {
            reservation.Status = status;
            return this;
        }

        public ReservationBuilder WithDateAndDuration(LocalDate date, int durationInDays)
        {
            reservation.Date = date;
            reservation.DurationInDays = durationInDays;
            return this;
        }

        public ReservationBuilder WithSentEmails(ReservationEmails sentEmails)
        {
            reservation.SentEmails = sentEmails;
            return this;
        }

        public Reservation Build() => reservation;
    }
}
