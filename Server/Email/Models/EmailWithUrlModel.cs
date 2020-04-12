using System;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailWithUrlModel : EmailWithNameModel
    {
        public EmailWithUrlModel(string from, Uri fromUrl, string name, Uri url) : base(from, fromUrl, name)
            => Url = url ?? throw new ArgumentNullException(nameof(url));

        public Uri Url { get; }
    }
}