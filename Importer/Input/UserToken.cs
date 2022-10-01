using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Importer.Input;

public class UserToken : IdentityUserToken<int>
{
    public virtual User? User { get; set; }
}