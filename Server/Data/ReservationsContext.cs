﻿using System;
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

        public DbSet<AccountBalance> AccountBalances { get; set; } = null!;
        public DbSet<Apartment> Apartments { get; set; } = null!;
        public DbSet<CleaningTask> CleaningTasks { get; set; } = null!;
        public DbSet<KeyCode> KeyCodes { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Posting> Postings { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<ReservedDay> ReservedDays { get; set; } = null!;
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<TransactionAmount> TransactionAmounts { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            if (modelBuilder is null)
                throw new ArgumentNullException(nameof(modelBuilder));

            base.OnModelCreating(modelBuilder);

            var instantConverter = new ValueConverter<Instant, DateTime>(
                instant => instant.ToDateTimeUtc(),
                dateTime => Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)));
            var localDateConverter = new ValueConverter<LocalDate, DateTime>(
                localDate => localDate.ToDateTimeUnspecified(),
                dateTime => LocalDate.FromDateTime(dateTime));

            modelBuilder.Entity<AccountBalance>()
                .HasKey(accountBalance => new { accountBalance.UserId, accountBalance.Account });

            modelBuilder.Entity<CleaningTask>()
                .Property(cleaningTask => cleaningTask.Date)
                .HasConversion(localDateConverter);

            modelBuilder.Entity<KeyCode>()
                .Property(keyCode => keyCode.Date)
                .HasConversion(localDateConverter);

            modelBuilder.Entity<Order>(builder =>
            {
                builder.Property(order => order.CreatedTimestamp)
                    .HasConversion(instantConverter);

                builder.HasMany(order => order.Transactions)
                    .WithOne(transaction => transaction.Order!);
            });

            modelBuilder.Entity<Posting>()
                .Property(transaction => transaction.Date)
                .HasConversion(localDateConverter);

            modelBuilder.Entity<Reservation>()
                .Property(reservation => reservation.UpdatedTimestamp)
                .HasConversion(instantConverter);
            modelBuilder.Entity<Reservation>()
                .Property(reservation => reservation.Date)
                .HasConversion(localDateConverter);
            modelBuilder.Entity<Reservation>()
                .OwnsOne(reservation => reservation.Price);

            modelBuilder.Entity<ReservedDay>()
                .Property(reservedDay => reservedDay.Date)
                .HasConversion(localDateConverter);
            modelBuilder.Entity<ReservedDay>()
                .HasIndex(reservedDay => new { reservedDay.Date, reservedDay.ResourceId })
                .IsUnique();

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

            modelBuilder.Entity<Transaction>(builder =>
            {
                builder.Property(transaction => transaction.Date)
                    .HasConversion(localDateConverter);
                builder.Property(transaction => transaction.Timestamp)
                    .HasConversion(instantConverter);
                builder.Property(transaction => transaction.ReservationDate)
                    .HasConversion(localDateConverter);

                builder.HasMany(transaction => transaction.Amounts)
                    .WithOne(amount => amount.Transaction!)
                    .HasForeignKey(amount => amount.TransactionId)
                    .IsRequired();
            });

            modelBuilder.Entity<TransactionAmount>()
                .HasKey(transactionAmount => new { transactionAmount.TransactionId, transactionAmount.Account });

            modelBuilder.Entity<User>(builder =>
            {
                builder.Property(user => user.Created)
                    .HasConversion(instantConverter);
                builder.Property(user => user.LatestSignIn)
                    .HasConversion(instantConverter);

                builder.HasMany(user => user.AccountBalances)
                    .WithOne(accountBalance => accountBalance.User!)
                    .HasForeignKey(accountBalance => accountBalance.UserId)
                    .IsRequired();

                builder.HasMany(user => user.Claims)
                    .WithOne(userClaim => userClaim.User!)
                    .HasForeignKey(userClaim => userClaim.UserId)
                    .IsRequired();

                builder.HasMany(user => user.Logins)
                    .WithOne(userLogin => userLogin.User!)
                    .HasForeignKey(userLogin => userLogin.UserId)
                    .IsRequired();

                builder.HasMany(user => user.Orders)
                    .WithOne(order => order.User!)
                    .HasForeignKey(user => user.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(user => user.Postings)
                    .WithOne(posting => posting.User!)
                    .HasForeignKey(posting => posting.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(user => user.Tokens)
                    .WithOne(userToken => userToken.User!)
                    .HasForeignKey(userToken => userToken.UserId)
                    .IsRequired();

                builder.HasMany(user => user.Transactions)
                    .WithOne(transaction => transaction.User!)
                    .HasForeignKey(user => user.UserId)
                    .OnDelete(DeleteBehavior.SetNull);

                builder.HasMany(user => user.UserRoles)
                    .WithOne(userRole => userRole.User!)
                    .HasForeignKey(userRole => userRole.UserId)
                    .IsRequired();
            });
        }
    }
}