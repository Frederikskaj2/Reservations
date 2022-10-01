using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Importer.Input;

public class UserLogin : IdentityUserLogin<int>
{
    public virtual User? User { get; set; }
}