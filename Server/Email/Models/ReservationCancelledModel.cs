using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class ReservationCancelledModel : ReservationModel
    {
        public ReservationCancelledModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, string resourceName, LocalDate date,
            int total, int cancellationFee) : base(from, fromUrl, name, url, orderId, resourceName, date)
        {
            if (total < 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));
            if (cancellationFee < 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));

            Total = total;
            CancellationFee = cancellationFee;
        }

        public int Total { get; }
        public int CancellationFee { get; }
    }
}