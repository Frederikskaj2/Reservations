using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public class ReservationsContext : IdentityDbContext<User, Role, int, UserClaim, UserRole, UserLogin, RoleClaim, UserToken>
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(builder =>
            {
                builder.HasMany(user => user.Claims)
                    .WithOne(userClaim => userClaim.User!)
                    .HasForeignKey(userClaim => userClaim.UserId)
                    .IsRequired();

                builder.HasMany(user => user.Logins)
                    .WithOne(userLogin => userLogin.User!)
                    .HasForeignKey(userLogin => userLogin.UserId)
                    .IsRequired();

                builder.HasMany(user => user.Tokens)
                    .WithOne(userToken => userToken.User!)
                    .HasForeignKey(userToken => userToken.UserId)
                    .IsRequired();

                builder.HasMany(user => user.UserRoles)
                    .WithOne(userRole => userRole.User!)
                    .HasForeignKey(userRole => userRole.UserId)
                    .IsRequired();
            });

            modelBuilder.Entity<Role>(builder =>
            {
                builder.HasMany(role => role.UserRoles)
                    .WithOne(userRole => userRole.Role!)
                    .HasForeignKey(userRole => userRole.RoleId)
                    .IsRequired();

                builder.HasMany(role => role.RoleClaims)
                    .WithOne(roleClaim => roleClaim.Role!)
                    .HasForeignKey(roleClaim => roleClaim.RoleId)
                    .IsRequired();
            });

            var instantConverter = new ValueConverter<Instant, DateTime>(
                instant => instant.ToDateTimeUtc(),
                dateTime => Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)));
            var localDateConverter = new ValueConverter<LocalDate, DateTime>(
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
        }
    }
}