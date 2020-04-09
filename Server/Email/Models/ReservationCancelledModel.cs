using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class ReservationCancelledModel : ReservationModel
    {
        public ReservationCancelledModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, string resourceName, LocalDate date,
            int deposit, int cancellationFee) : base(from, fromUrl, name, url, orderId, resourceName, date)
        {
            if (deposit <= 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));
            if (cancellationFee < 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));

            Deposit = deposit;
            CancellationFee = cancellationFee;
        }

        public int Deposit { get; }
        public int CancellationFee { get; }
    }
}