using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Identity;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    [SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Entity Framework require an accesible setter.")]
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

        [Timestamp]
        [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "This property is only used by the framework.")]
        public byte[]? Timestamp { get; set; }
    }
}