using System;
using System.Collections.Generic;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class LockBoxCodeModel : OrderModel
    {
        public LockBoxCodeModel(
            string from, Uri fromUrl, string name, Uri url, int orderId, string resourceName, LocalDate date,
            List<DatedLockBoxCode> datedLockBoxCodes, Uri rulesUri) : base(from, fromUrl, name, url, orderId)
        {
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("Value cannot be null or empty.", nameof(resourceName));

            ResourceName = resourceName;
            Date = date;
            DatedLockBoxCodes = datedLockBoxCodes ?? throw new ArgumentNullException(nameof(datedLockBoxCodes));
            RulesUri = rulesUri ?? throw new ArgumentNullException(nameof(rulesUri));
        }

        public string ResourceName { get; }
        public LocalDate Date { get; }
        public List<DatedLockBoxCode> DatedLockBoxCodes { get; }
        public Uri RulesUri { get; }
    }
}