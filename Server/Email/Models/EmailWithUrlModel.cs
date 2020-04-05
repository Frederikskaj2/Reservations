using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailWithUrlModel : EmailModel
    {
        public EmailWithUrlModel(string from, Uri fromUrl, string name, Uri url) : base(from, fromUrl)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Value cannot be null or empty.", nameof(name));

            Name = name;
            Url = url ?? throw new ArgumentNullException(nameof(url));
        }

        public string Name { get; }
        public Uri Url { get; }
    }
}