using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Importer.Input;

public class UserRole : IdentityUserRole<int>
{
    public virtual User? User { get; set; }
    public virtual Role? Role { get; set; }
}