using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Importer.Input;

public class RoleClaim : IdentityRoleClaim<int>
{
    public virtual Role? Role { get; set; }
}