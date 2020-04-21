using System;
using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class KeyCodeModel : OrderModel
    {
        public KeyCodeModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, string resourceName, LocalDate date,
            List<DatedKeyCode> datedKeyCodes) : base(from, fromUrl, name, url, orderId)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));

            ResourceName = resourceName;
            Date = date;
            DatedKeyCodes = datedKeyCodes ?? throw new ArgumentNullException(nameof(datedKeyCodes));
        }

        public string ResourceName { get; }
        public LocalDate Date { get; }
        public List<DatedKeyCode> DatedKeyCodes { get; }
    }
}