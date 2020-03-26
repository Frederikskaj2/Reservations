using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class UserClaim : IdentityUserClaim<int>
    {
        public virtual User? User { get; set; }
    }
}