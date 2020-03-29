using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; } = string.Empty;
        public bool IsPendingDelete { get; set; }
        public int? ApartmentId { get; set; }
        public Apartment? Apartment { get; set; }
        public List<Order>? Orders { get; set; }
        public virtual ICollection<UserClaim>? Claims { get; set; }
        public virtual ICollection<UserLogin>? Logins { get; set; }
        public virtual ICollection<UserToken>? Tokens { get; set; }
        public virtual ICollection<UserRole>? UserRoles { get; set; }
        public virtual ICollection<Transaction>? Transactions { get; set; }
    }
}