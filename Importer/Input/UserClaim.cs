using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Importer.Input;

public class UserClaim : IdentityUserClaim<int>
{
    public virtual User? User { get; set; }
}