using System;
using User = Frederikskaj2.Reservations.Server.Data.User;

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