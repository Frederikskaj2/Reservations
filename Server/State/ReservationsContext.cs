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
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<ResourceReservation> ResourceReservations { get; set; } = null!;
        public DbSet<Apartment> Apartments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
        }
    }
}