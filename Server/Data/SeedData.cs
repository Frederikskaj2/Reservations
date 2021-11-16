using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.ErrorHandling;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    public sealed class SeedData : IDisposable
    {
        private readonly IClock clock;
        private readonly ReservationsContext db;
        private readonly SeedDataOptions options;
        private readonly RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
        private readonly RoleManager<Role> roleManager;
        private readonly UserManager<User> userManager;

        public SeedData(
            IOptions<SeedDataOptions> options, IClock clock, ReservationsContext db,
            RoleManager<Role> roleManager, UserManager<User> userManager)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.clock = clock ?? throw new ArgumentNullException(nameof(clock));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));

            this.options = options.Value;

            if (!(this.options.Users?.Any() ?? false))
                throw new ConfigurationException("Missing seed data users.");
            foreach (var user in this.options.Users)
            {
                if (string.IsNullOrEmpty(user.Email))
                    throw new ConfigurationException("Missing seed data user email.");
                if (string.IsNullOrEmpty(user.FullName))
                    throw new ConfigurationException("Missing seed data user full name.");
                if (string.IsNullOrEmpty(user.Phone))
                    throw new ConfigurationException("Missing seed data user phone.");
            }

            if (!(this.options.Resources?.Any() ?? false))
                throw new ConfigurationException("Missing seed data resources.");
            foreach (var resource in this.options.Resources)
                if (string.IsNullOrEmpty(resource.Name))
                    throw new ConfigurationException("Missing seed data resource name.");
        }

        public void Dispose() => randomNumberGenerator.Dispose();

        public async Task Initialize()
        {
            if (!await db.Database.EnsureCreatedAsync())
            {
                await FixTransaction782();
#if false
                await UpdateUsers();
#endif
                return;
            }

            await CreateTriggers();

            CreateApartments();
            CreateResources();
            await db.SaveChangesAsync();

            await CreateRoles();
            await CreateUsers();
        }

        private async Task CreateTriggers()
        {
            await CreateTrigger(nameof(ReservationsContext.AccountBalances), nameof(AccountBalance.Timestamp));
            await CreateTrigger(nameof(ReservationsContext.Orders), nameof(Order.Timestamp));
            await CreateTrigger(nameof(ReservationsContext.Reservations), nameof(Reservation.Timestamp));
            await CreateTrigger("AspNetUsers", nameof(User.Timestamp));
        }

        private async Task CreateTrigger(string tableName, string columnName)
        {
            const string triggerFormat = @"CREATE TRIGGER Set{0}{1}On{2}
AFTER {2} ON {0}
BEGIN
    UPDATE {0}
    SET {1} = randomblob(8)
    WHERE rowid = NEW.rowid;
END";
            await db.Database.ExecuteSqlRawAsync(string.Format(CultureInfo.InvariantCulture, triggerFormat, tableName, columnName, "Insert"));
            await db.Database.ExecuteSqlRawAsync(string.Format(CultureInfo.InvariantCulture, triggerFormat, tableName, columnName, "Update"));
        }

        private void CreateApartments()
        {
            var apartments = GetApartments();
            db.Apartments.AddRange(apartments);
        }

        private static IEnumerable<Apartment> GetApartments()
        {
            var stories = new Dictionary<char, int>
            {
                { 'A', 5 },
                { 'B', 4 },
                { 'C', 5 },
                { 'D', 3 },
                { 'E', 6 },
                { 'F', 3 },
                { 'G', 4 },
                { 'H', 4 },
                { 'K', 5 },
                { 'L', 5 },
                { 'M', 5 },
                { 'N', 3 },
                { 'P', 4 },
                { 'R', 6 }
            };
            return stories.SelectMany(
                kvp => Enumerable.Range(0, kvp.Value + 1)
                    .SelectMany(story => new[]
                        {
                            new Apartment { Letter = kvp.Key, Story = story, Side = ApartmentSide.Left },
                            new Apartment { Letter = kvp.Key, Story = story, Side = ApartmentSide.Right }
                        }))
                .Concat(new[]
                {
                    new Apartment { Letter = 'E', Story = 7 },
                    new Apartment { Letter = 'L', Story = 6 },
                    new Apartment { Letter = 'L', Story = 7 },
                    new Apartment { Letter = 'R', Story = 7 },
                    new Apartment { Letter = 'V', Story = -1 },
                    new Apartment { Letter = 'W', Story = -1 }
                })
                .OrderBy(apartment => apartment.Letter)
                .ThenBy(apartment => apartment.Story)
                .ThenBy(apartment => apartment.Side);
        }

        private void CreateResources()
        {
            var resources = options.Resources
                .Select((resource, i) => new Resource
                {
                    Sequence = i + 1,
                    Type = resource.Type,
                    Name = resource.Name!,
                    LockBoxCodes = (resource.LockBoxCodes ?? Enumerable.Empty<LockBoxCodeOptions>())
                        .Select(code => new LockBoxCode { Date = code.Date, Code = code.Code! })
                        .ToList()
                });
            db.Resources.AddRange(resources);
        }

        private async Task CreateRoles()
        {
            var roles = typeof(Roles)
                .GetFields(BindingFlags.Public | BindingFlags.Static)
                .Where(field => field.IsLiteral && field.FieldType == typeof(string))
                .Select(field => new Role { Name = (string?)field.GetValue(null) });
            foreach (var role in roles)
                await roleManager.CreateAsync(role);
        }

        private async Task CreateUsers()
        {
            var now = clock.GetCurrentInstant();
            foreach (var userOptions in options.Users!)
            {
                var user = new User
                {
                    UserName = userOptions!.Email,
                    Email = userOptions.Email,
                    EmailConfirmed = true,
                    FullName = userOptions.FullName!,
                    PhoneNumber = userOptions.Phone,
                    EmailSubscriptions = userOptions.EmailSubscriptions,
                    Created = now,
                    LatestSignIn = now
                };
                var password = !string.IsNullOrEmpty(userOptions.Password)
                    ? userOptions.Password
                    : CreateRandomPassword();
                await userManager.CreateAsync(user, password);
                if (userOptions.Roles == null)
                    continue;
                foreach (var role in userOptions.Roles)
                    await userManager.AddToRoleAsync(user, role);
            }
        }

        private string CreateRandomPassword()
        {
            var bytes = new byte[18];
            randomNumberGenerator.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        private async Task FixTransaction782()
        {
            // The user created a new order but didn't pay it (255).
            //
            // Then the user asked to cancel a previously paid order (205). The
            // refunded amount (1400) got "stuck" because the accounts
            // receivable of the user at that point was 2400 which resulted in
            // the 1400 being subtracted ending in a balance of 1000.
            //
            // However, the 1400 wasn't transferred from order 205 to 255 but as
            // the 1400 was applied to accounts receivable instead of payments
            // they were not scheduled for pay out.
            //
            // From the user's point of view they only owe 1000 so that should
            // be enough to confirm order 255. Even if they paid the entire 2400
            // on 255 the 1400 from 205 was still "stuck" and would not get
            // scheduled for pay out.
            //
            // To fix this the 205 cancel transaction is changed to apply the
            // refunded amount to the Payments account instead of the
            // AccountsReceivable account.
            //
            // Then two transactions are created to transfer the payment from
            // 205 to 255.
            //
            // That this can happen is a bug in the system. Cancelling paid
            // orders while there are unpaid orders doesn't work correctly. The
            // logic in this method should be implemented more generally.
            const int transactionId = 782;
            const int orderId = 255;

            var oldTransactionAmount = await db.TransactionAmounts.SingleOrDefaultAsync(ta => ta.TransactionId == transactionId && ta.Account == Account.AccountsReceivable);
            if (oldTransactionAmount == null)
                return;
            var transaction = await db.Transactions.SingleOrDefaultAsync(t => t.Id == transactionId);
            var order = await db.Orders.SingleAsync(o => o.Id == orderId);
            var accountBalances = await db.AccountBalances.Where(ab => ab.UserId == order.UserId).ToListAsync();

            var newTransactionAmount = new TransactionAmount
            {
                TransactionId = transactionId,
                Account = Account.Payments,
                Amount = oldTransactionAmount.Amount
            };
            db.TransactionAmounts.Remove(oldTransactionAmount);
            RemoveTransactionAmount(oldTransactionAmount);
            db.TransactionAmounts.Add(newTransactionAmount);
            AddTransactionAmount(newTransactionAmount);

            var newTransaction1 = new Transaction
            {
                Date = transaction.Date,
                CreatedByUserId = transaction.CreatedByUserId,
                Timestamp = transaction.Timestamp,
                UserId = transaction.UserId,
                OrderId = transaction.OrderId,
                Amounts = new List<TransactionAmount>
                {
                    new TransactionAmount { Account = Account.Payments, Amount = -oldTransactionAmount.Amount },
                    new TransactionAmount { Account = Account.ToAccountsReceivable, Amount = oldTransactionAmount.Amount },
                }
            };
            db.Transactions.Add(newTransaction1);
            AddTransactionAmount(newTransaction1.Amounts.First());
            AddTransactionAmount(newTransaction1.Amounts.ElementAt(1));

            var newTransaction2 = new Transaction
            {
                Date = transaction.Date,
                CreatedByUserId = transaction.CreatedByUserId,
                Timestamp = transaction.Timestamp,
                UserId = transaction.UserId,
                OrderId = orderId,
                Amounts = new List<TransactionAmount>
                {
                    new TransactionAmount { Account = Account.AccountsReceivable, Amount = oldTransactionAmount.Amount },
                    new TransactionAmount { Account = Account.FromPayments, Amount = -oldTransactionAmount.Amount },
                }
            };
            db.Transactions.Add(newTransaction2);
            AddTransactionAmount(newTransaction2.Amounts.First());
            AddTransactionAmount(newTransaction2.Amounts.ElementAt(1));

            await db.SaveChangesAsync();

            void RemoveTransactionAmount(TransactionAmount transactionAmount) =>
                accountBalances.Single(ab => ab.Account == transactionAmount.Account).Amount -= transactionAmount.Amount;

            void AddTransactionAmount(TransactionAmount transactionAmount) =>
                accountBalances.Single(ab => ab.Account == transactionAmount.Account).Amount += transactionAmount.Amount;
        }

#if false
        private async Task UpdateUsers()
        {
            var now = clock.GetCurrentInstant();

            async Task CreateUser(string email, string fullName, string phone, EmailSubscriptions emailSubscriptions, params string[] roles)
            {
                var existingUser = await userManager.FindByEmailAsync(email);
                if (existingUser != null)
                    return;

                var user = new User
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                    FullName = fullName,
                    PhoneNumber = phone,
                    EmailSubscriptions = emailSubscriptions,
                    Created = now,
                    LatestSignIn = now,
                };
                var password = CreateRandomPassword();
                await userManager.CreateAsync(user, password);
                foreach (var role in roles)
                    await userManager.AddToRoleAsync(user, role);
            }
        }
#endif
    }
}