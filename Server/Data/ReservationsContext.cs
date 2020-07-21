using System;
using System.Diagnostics.CodeAnalysis;
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
        public DbSet<LockBoxCode> LockBoxCodes { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<Posting> Postings { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;
        public DbSet<ReservedDay> ReservedDays { get; set; } = null!;
        public DbSet<Resource> Resources { get; set; } = null!;
        public DbSet<Transaction> Transactions { get; set; } = null!;
        public DbSet<TransactionAmount> TransactionAmounts { get; set; } = null!;

        public DbSet<KeyCode> KeyCodes { get; set; } = null!;

        [SuppressMessage("Microsoft.Maintainability", "CA1506:Avoid excessive class coupling", Justification = "This class is naturally coupled to many different classes.")]
        protected override void OnModelCreating(ModelBuilder builder)
        {
            if (builder is null)
                throw new ArgumentNullException(nameof(builder));

            base.OnModelCreating(builder);

            var instantConverter = new ValueConverter<Instant, DateTime>(
                instant => instant.ToDateTimeUtc(),
                dateTime => Instant.FromDateTimeUtc(DateTime.SpecifyKind(dateTime, DateTimeKind.Utc)));
            var localDateConverter = new ValueConverter<LocalDate, DateTime>(
                localDate => localDate.ToDateTimeUnspecified(),
                dateTime => LocalDate.FromDateTime(dateTime));

            builder.Entity<AccountBalance>()
                .HasKey(accountBalance => new { accountBalance.UserId, accountBalance.Account });

            builder.Entity<CleaningTask>()
                .Property(cleaningTask => cleaningTask.Date)
                .HasConversion(localDateConverter);

            builder.Entity<LockBoxCode>()
                .Property(lockBoxCode => lockBoxCode.Date)
                .HasConversion(localDateConverter);

            builder.Entity<Order>(builder =>
            {
                builder.Property(order => order.CreatedTimestamp)
                    .HasConversion(instantConverter);

                builder.HasMany(order => order.Transactions)
                    .WithOne(transaction => transaction.Order!);
            });

            builder.Entity<Posting>()
                .Property(transaction => transaction.Date)
                .HasConversion(localDateConverter);

            builder.Entity<Reservation>()
                .Property(reservation => reservation.UpdatedTimestamp)
                .HasConversion(instantConverter);
            builder.Entity<Reservation>()
                .Property(reservation => reservation.Date)
                .HasConversion(localDateConverter);
            builder.Entity<Reservation>()
                .OwnsOne(reservation => reservation.Price);

            builder.Entity<ReservedDay>()
                .Property(reservedDay => reservedDay.Date)
                .HasConversion(localDateConverter);
            builder.Entity<ReservedDay>()
                .HasIndex(reservedDay => new { reservedDay.Date, reservedDay.ResourceId })
                .IsUnique();

            builder.Entity<Role>(builder =>
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

            builder.Entity<Transaction>(builder =>
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

            builder.Entity<TransactionAmount>()
                .HasKey(transactionAmount => new { transactionAmount.TransactionId, transactionAmount.Account });

            builder.Entity<User>(builder =>
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

            builder.Entity<KeyCode>()
                .Property(keyCode => keyCode.Date)
                .HasConversion(localDateConverter);
        }
    }
}