using System;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class ConfirmEmailModel
    {
        public ConfirmEmailModel(User user, Uri confirmEmailUrl, string from, Uri fromUrl)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));
            ConfirmEmailUrl = confirmEmailUrl ?? throw new ArgumentNullException(nameof(confirmEmailUrl));
            From = from ?? throw new ArgumentNullException(nameof(from));
            FromUrl = fromUrl ?? throw new ArgumentNullException(nameof(fromUrl));
        }

        public User User { get; }
        public Uri ConfirmEmailUrl { get; }
        public string From { get; }
        public Uri FromUrl { get; }
    }
}