using System;
using System.Collections.Generic;
using System.Linq;
using Frederikskaj2.Reservations.Server.Passwords;
using Frederikskaj2.Reservations.Shared;
using Microsoft.Extensions.Configuration;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Data
{
    internal class SeedData
    {
        private readonly ReservationsContext db;
        private readonly IPasswordHasher passwordHasher;
        private readonly IConfiguration configuration;

        public SeedData(ReservationsContext db, IPasswordHasher passwordHasher, IConfiguration configuration)
        {
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            this.configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public void Initialize()
        {
            if (!db.Database.EnsureCreated())
                return;

            var resources = new[]
            {
                new Resource { Sequence = 1, Type = ResourceType.BanquetFacilities, Name = "Fest-/aktivitetslokale" },
                new Resource { Sequence = 2, Type = ResourceType.Bedroom, Name = "Frederik (soveværelse)" },
                new Resource { Sequence = 3, Type = ResourceType.Bedroom, Name = "Kaj (soveværelse)" }
            };
            db.Resources.AddRange(resources);

            var apartments = GetApartments().ToList();
            db.Apartments.AddRange(apartments);

            var password = configuration.GetValue<string>("ReservationsAdministratorPassword");
            var user = new User
            {
                Email = "martin@liversage.com",
                FullName = "Martin Liversage",
                Phone = "31 18 97 93",
                HashedPassword = passwordHasher.HashPassword(password),
                IsAdministrator = true
            };
            db.Users.Add(user);

            var random = new Random();
            var users = new[]
            {
                new User { Email = "a@frederikskaj2.dk", FullName = "A F", Phone = CreatePhone(), Apartment = apartments[random.Next(apartments.Count)] },
                new User { Email = "b@frederikskaj2.dk", FullName = "B F", Phone = CreatePhone(), Apartment = apartments[random.Next(apartments.Count)] },
                new User { Email = "c@frederikskaj2.dk", FullName = "C F", Phone = CreatePhone(), Apartment = apartments[random.Next(apartments.Count)] },
            };
            db.Users.AddRange(users);

            string CreatePhone() => random.Next(21000000, 70000000 - 21000000).ToString();

            var timeZoneInfo = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;
            var now = SystemClock.Instance.GetCurrentInstant().InZone(timeZoneInfo);
            var reservedDays = new Dictionary<(LocalDate, Resource), ReservedDay>();

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

            db.Orders.AddRange(Enumerable.Range(0, 20).Select(_ => CreateOrder()));

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

            db.SaveChanges();
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