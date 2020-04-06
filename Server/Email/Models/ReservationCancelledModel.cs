using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class ReservationCancelledModel : ReservationModel
    {
        public ReservationCancelledModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, string resourceName, LocalDate date,
            int cancellationFee) : base(from, fromUrl, name, url, orderId, resourceName, date)
        {
            if (cancellationFee < 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));
            CancellationFee = cancellationFee;
        }

        public int CancellationFee { get; }
    }
}