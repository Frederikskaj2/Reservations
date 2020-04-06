using System;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public abstract class ReservationModel : OrderModel
    {
        protected ReservationModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, string resourceName, LocalDate date)
            : base(from, fromUrl, name, url, orderId)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));

            ResourceName = resourceName;
            Date = date;
        }

        public string ResourceName { get; }
        public LocalDate Date { get; }
    }
}