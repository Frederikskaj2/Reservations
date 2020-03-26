using System;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class EmailWithTokenModel
    {
        public EmailWithTokenModel(User user, Uri confirmEmailUrl, string from, Uri fromUrl)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            Url = confirmEmailUrl ?? throw new ArgumentNullException(nameof(confirmEmailUrl));
            From = from ?? throw new ArgumentNullException(nameof(from));
            FromUrl = fromUrl ?? throw new ArgumentNullException(nameof(fromUrl));
        }

        public User User { get; }
        public Uri Url { get; }
        public string From { get; }
        public Uri FromUrl { get; }
    }
}