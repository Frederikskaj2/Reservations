using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;

namespace Frederikskaj2.Reservations.Server.State
{
    public class ReservationsContext : DbContext
    {
        public ReservationsContext()
        {
        }

        public ReservationsContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
        }
    }
}