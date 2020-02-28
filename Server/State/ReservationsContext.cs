using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;

namespace Frederikskaj2.Reservations.Server.State
{
    public class ReservationsContext : DbContext
    {
        public DbSet<User> Users { get; set; } = null!;
    }
}