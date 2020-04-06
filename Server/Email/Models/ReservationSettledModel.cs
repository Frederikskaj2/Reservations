using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class ReservationSettledModel : ReservationModel
    {
        public ReservationSettledModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, string resourceName, LocalDate date,
            int deposit, int damages, string? damagesDescription)
            : base(from, fromUrl, name, url, orderId, resourceName, date)
        {
            if (deposit <= 0)
                throw new ArgumentOutOfRangeException(nameof(deposit));
            if (damages < 0)
                throw new ArgumentOutOfRangeException(nameof(damages));
            if (damages > 0 && string.IsNullOrEmpty(damagesDescription))
                throw new ArgumentException("Value cannot be null or empty.", nameof(damagesDescription));

            Deposit = deposit;
            Damages = damages;
            DamagesDescription = damagesDescription;
        }

        public int Deposit { get; }
        public int Damages { get; }
        public string? DamagesDescription { get; }
    }
}