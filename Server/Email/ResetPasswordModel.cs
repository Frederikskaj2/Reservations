using System;
using Frederikskaj2.Reservations.Shared;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class ResetPasswordModel
    {
        public ResetPasswordModel(User user, Uri url)
        {
            User = user;
            Url = url;
        }

        public User User { get; }
        public Uri Url { get; }
    }
}