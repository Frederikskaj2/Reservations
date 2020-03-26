using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class UserLogin : IdentityUserLogin<int>
    {
        public virtual User? User { get; set; }
    }
}