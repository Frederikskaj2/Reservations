using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class RoleClaim : IdentityRoleClaim<int>
    {
        public virtual Role? Role { get; set; }
    }
}