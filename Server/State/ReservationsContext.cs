using System;
using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

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

            var instantConverter =  new ValueConverter<Instant, DateTime>(
                instant => instant.ToDateTimeUtc(), 
                dateTime => Instant.FromDateTimeUtc(dateTime));
            var localDateConverter =  new ValueConverter<LocalDate, DateTime>(
                localDate => localDate.ToDateTimeUnspecified(), 
                dateTime => LocalDate.FromDateTime(dateTime));

            modelBuilder.Entity<Reservation>()
                .Property(reservation => reservation.CreatedTimestamp)
                .HasConversion(instantConverter);
            modelBuilder.Entity<Reservation>()
                .Property(reservation => reservation.UpdatedTimestamp)
                .HasConversion(instantConverter);

            modelBuilder.Entity<ResourceReservation>()
                .Property(resourceReservation => resourceReservation.Date)
                .HasConversion(localDateConverter);
        }
    }
}