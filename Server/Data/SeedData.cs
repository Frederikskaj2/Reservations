using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    internal class SeedData
    {
        private readonly ReservationsContext db;
        private readonly SeedDataOptions options;
        private readonly Random random = new Random();
        private readonly RoleManager<Role> roleManager;
        private readonly UserManager<User> userManager;

        public SeedData(IOptions<SeedDataOptions> options, ReservationsContext db, UserManager<User> userManager, RoleManager<Role> roleManager)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.roleManager = roleManager ?? throw new ArgumentNullException(nameof(roleManager));

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
                if (string.IsNullOrEmpty(user.Password))
                    throw new ConfigurationException("Missing seed data user password.");
            }
            if (!(this.options.Resources?.Any() ?? false))
                throw new ConfigurationException("Missing seed data resources.");
            foreach (var resource in this.options.Resources)
                if (string.IsNullOrEmpty(resource.Name))
                    throw new ConfigurationException("Missing seed data resource name.");
        }

        public async Task Initialize()
        {
            if (!db.Database.EnsureCreated())
                return;

            var apartments = CreateApartments();
            var resources = CreateResources();
            CreateHolidays();
            await db.SaveChangesAsync();

            await CreateUsers();

            var users = new[]
            {
                CreateRandomUser("a@frederikskaj2.dk", "A F"),
                CreateRandomUser("b@frederikskaj2.dk", "B F"),
                CreateRandomUser("c@frederikskaj2.dk", "C F")
            };
            foreach (var user in users)
                await userManager.CreateAsync(user);

            var timeZoneInfo = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;
            var reservedDays = new Dictionary<(LocalDate, Resource), ReservedDay>();

            db.Orders.AddRange(Enumerable.Range(0, 20).Select(_ => CreateOrder()));

            await db.SaveChangesAsync();

            User CreateRandomUser(string email, string fullName) => new User
            {
                UserName = email,
                Email = email,
                EmailConfirmed = true,
                FullName = fullName,
                PhoneNumber = random.Next(21000000, 70000000 - 21000000).ToString(),
                Apartment = apartments[random.Next(apartments.Length)]
            };

            Order CreateOrder()
            {
                const int secondsPerDay = 24*60*60;
                var user = users[random.Next(users.Length)];
                var timestamp = SystemClock.Instance.GetCurrentInstant()
                    .Minus(Duration.FromSeconds(random.Next(6*secondsPerDay, 45*secondsPerDay)));
                var order = new Order
                {
                    User = user,
                    Apartment = user.Apartment,
                    CreatedTimestamp = timestamp,
                    Reservations = new List<Reservation>()
                };
                var reservationCount = 2 - (int) Math.Log2(random.Next(1, 4));
                while (order.Reservations.Count < reservationCount)
                {
                    var reservation = CreateReservation(timestamp);
                    if (reservation.Days.Any(day => reservedDays.ContainsKey((day.Date, reservation.Resource!))))
                        continue;
                    order.Reservations.Add(reservation);
                    foreach (var day in reservation.Days!)
                        reservedDays.Add((day.Date, reservation.Resource!), day);
                }
                return order;
            }

            Reservation CreateReservation(Instant timestamp)
            {
                var resource = resources[random.Next(resources.Length)];
                // ReSharper disable once AssignNullToNotNullAttribute
                var date = SystemClock.Instance.GetCurrentInstant().InZone(timeZoneInfo).Date.PlusDays(random.Next(90) - 10);
                var durationInDays = 3 - (int) Math.Log2(random.Next(1, 8));
                return new Reservation
                {
                    Resource = resource,
                    Status = ReservationStatus.Reserved,
                    UpdatedTimestamp = timestamp,
                    Days = Enumerable.Range(0, durationInDays).Select(i => new ReservedDay { Date = date.PlusDays(i), Resource = resource }).ToList()
                };
            }
        }

        private async Task CreateUsers()
        {
            var administratorRole = new Role { Name = Roles.Administrator };
            await roleManager.CreateAsync(administratorRole);

            foreach (var user in options.Users!)
            {
                var u = new User
                {
                    UserName = user!.Email,
                    Email = user.Email,
                    EmailConfirmed = true,
                    FullName = user.FullName!,
                    PhoneNumber = user.Phone
                };
                await userManager.CreateAsync(u, user.Password);
                if (user.IsAdministrator)
                    await userManager.AddToRoleAsync(u, Roles.Administrator);
            }
        }

        private Resource[] CreateResources()
        {
            var resources = options.Resources
                .Select((resource, i) => new Resource { Sequence = i + 1, Type = resource.Type, Name = resource.Name! })
                .ToArray();
            db.Resources.AddRange(resources);
            return resources;
        }

        private Holiday[] CreateHolidays()
        {
            var holidays = new[]
            {
                new Holiday { Date = new LocalDate(2020, 4, 9) },
                new Holiday { Date = new LocalDate(2020, 4, 10) },
                new Holiday { Date = new LocalDate(2020, 4, 13) },
                new Holiday { Date = new LocalDate(2020, 5, 8) },
                new Holiday { Date = new LocalDate(2020, 5, 21) },
                new Holiday { Date = new LocalDate(2020, 6, 1) },
                new Holiday { Date = new LocalDate(2020, 12, 24) },
                new Holiday { Date = new LocalDate(2020, 12, 25) },
                new Holiday { Date = new LocalDate(2020, 12, 31) },
            };
            db.Holidays.AddRange(holidays);
            return holidays;
        }

        private Apartment[] CreateApartments()
        {
            var apartments = GetApartments().ToArray();
            db.Apartments.AddRange(apartments);
            return apartments;
        }

        private static IEnumerable<Apartment> GetApartments()
        {
            var stories = new Dictionary<char, Stories>
            {
                { 'A', new Stories { Left = 5, Right = 5 } },
                { 'B', new Stories { Left = 4, Right = 4 } },
                { 'C', new Stories { Left = 5, Right = 5 } },
                { 'D', new Stories { Left = 3, Right = 3 } },
                { 'E', new Stories { Left = 6, Right = 7 } },
                { 'F', new Stories { Left = 3, Right = 3 } },
                { 'G', new Stories { Left = 4, Right = 4 } },
                { 'H', new Stories { Left = 4, Right = 4 } },
                { 'K', new Stories { Left = 5, Right = 5 } },
                { 'L', new Stories { Left = 6, Right = 7 } },
                { 'M', new Stories { Left = 5, Right = 5 } },
                { 'P', new Stories { Left = 4, Right = 4 } },
                { 'R', new Stories { Left = 6, Right = 7 } },
            };
            return stories.SelectMany(
                    kvp =>
                        Enumerable.Range(0, kvp.Value.Left + 1)
                            .Select(story => new Apartment { Letter = kvp.Key, Story = story, Side = ApartmentSide.Left })
                            .Concat(Enumerable.Range(0, kvp.Value.Right + 1)
                                .Select(story => new Apartment { Letter = kvp.Key, Story = story, Side = ApartmentSide.Right })))
                .OrderBy(apartment => apartment.Letter)
                .ThenBy(apartment => apartment.Story)
                .ThenBy(apartment => apartment.Side);
        }

        private class Stories
        {
            public int Left { get; set; }
            public int Right { get; set; }
        }
    }
}