using System;
using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Domain
{
    public class ReservationsContext : DbContext
    {
        public ReservationsContext()
        {
        }

        public ReservationsContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Apartment> Apartments { get; set; } = null!;
        public DbSet<Holiday> Holidays { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var instantConverter =  new ValueConverter<Instant, DateTime>(
                instant => instant.ToDateTimeUtc(), 
                dateTime => Instant.FromDateTimeUtc(dateTime));
            var localDateConverter =  new ValueConverter<LocalDate, DateTime>(
                localDate => localDate.ToDateTimeUnspecified(), 
                dateTime => LocalDate.FromDateTime(dateTime));

            modelBuilder.Entity<Holiday>()
                .Property(holiday => holiday.Date)
                .HasConversion(localDateConverter);

            modelBuilder.Entity<Order>()
                .Property(reservation => reservation.CreatedTimestamp)
                .HasConversion(instantConverter);
            modelBuilder.Entity<Order>()
                .Property(reservation => reservation.UpdatedTimestamp)
                .HasConversion(instantConverter);

            modelBuilder.Entity<Reservation>()
                .Property(resourceReservation => resourceReservation.Date)
                .HasConversion(localDateConverter);

            modelBuilder.Entity<User>().HasIndex(user => user.Email).IsUnique();
        }
    }
}