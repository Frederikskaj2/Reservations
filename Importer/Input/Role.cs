using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Importer.Input;

public class Role : IdentityRole<int>
{
    public virtual ICollection<UserRole>? UserRoles { get; set; }
    public virtual ICollection<RoleClaim>? RoleClaims { get; set; }
}