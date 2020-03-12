using System;
using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
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
        public DbSet<ReservedDay> ReservedDays { get; set; } = null!;
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var instantConverter =  new ValueConverter<Instant, DateTime>(
                instant => instant.ToDateTimeUtc(),
                dateTime => Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)));
            var localDateConverter =  new ValueConverter<LocalDate, DateTime>(
                localDate => localDate.ToDateTimeUnspecified(),
                dateTime => LocalDate.FromDateTime(dateTime));

            modelBuilder.Entity<Holiday>()
                .Property(holiday => holiday.Date)
                .HasConversion(localDateConverter);

            modelBuilder.Entity<Order>()
                .Property(order => order.CreatedTimestamp)
                .HasConversion(instantConverter);

            modelBuilder.Entity<Reservation>()
                .Property(reservation => reservation.UpdatedTimestamp)
                .HasConversion(instantConverter);
            modelBuilder.Entity<Reservation>()
                .OwnsOne(reservation => reservation.Price);

            modelBuilder.Entity<ReservedDay>()
                .Property(reservedDay => reservedDay.Date)
                .HasConversion(localDateConverter);
            modelBuilder.Entity<ReservedDay>()
                .HasIndex(reservedDay => new { reservedDay.Date, reservedDay.ResourceId })
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(user => user.Email)
                .IsUnique();
        }
    }
}