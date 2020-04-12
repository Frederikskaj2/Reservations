using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public abstract class EmailModel
    {
        protected EmailModel(string from, Uri fromUrl)
        {
            if (string.IsNullOrEmpty(from))
                throw new ArgumentException("Value cannot be null or empty.", nameof(from));

            From = from;
            FromUrl = fromUrl ?? throw new ArgumentNullException(nameof(fromUrl));
        }

        public string From { get; }
        public Uri FromUrl { get; }
    }
}