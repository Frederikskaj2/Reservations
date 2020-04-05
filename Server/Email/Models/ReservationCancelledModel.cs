using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class ReservationCancelledModel : OrderModel
    {
        public ReservationCancelledModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, string resourceName, LocalDate date,
            int cancellationFee)
            : base(from, fromUrl, name, url, orderId)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));
            if (cancellationFee <= 0)
                throw new ArgumentOutOfRangeException(nameof(cancellationFee));

            ResourceName = resourceName;
            Date = date;
            CancellationFee = cancellationFee;
        }

        public string ResourceName { get; }
        public LocalDate Date { get; }
        public int CancellationFee { get; }
    }
}