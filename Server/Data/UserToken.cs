using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class UserToken : IdentityUserToken<int>
    {
        public virtual User? User { get; set; }
    }
}