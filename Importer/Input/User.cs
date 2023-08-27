using Microsoft.AspNetCore.Identity;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Importer.Input;

public class User : IdentityUser<int>
{
    public string FullName { get; set; } = string.Empty;
    public Instant Created { get; set; }
    public Instant LatestSignIn { get; set; }
    public bool IsPendingDelete { get; set; }
    public int? ApartmentId { get; set; }
    public Apartment? Apartment { get; set; }
    public string? AccountNumber { get; set; }
    public List<Order>? Orders { get; set; }
    public EmailSubscriptions EmailSubscriptions { get; set; }

    public virtual ICollection<AccountBalance>? AccountBalances { get; set; }
    public virtual ICollection<UserClaim>? Claims { get; set; }
    public virtual ICollection<UserLogin>? Logins { get; set; }
    public virtual ICollection<Posting>? Postings { get; set; }
    public virtual ICollection<UserToken>? Tokens { get; set; }
    public virtual ICollection<Transaction>? Transactions { get; set; }
    public virtual ICollection<UserRole>? UserRoles { get; set; }
}
