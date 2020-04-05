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
        private readonly RoleManager<Role> roleManager;
        private readonly UserManager<User> userManager;

        public SeedData(
            IOptions<SeedDataOptions> options, ReservationsContext db, UserManager<User> userManager,
            RoleManager<Role> roleManager)
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

            CreateApartments();
            CreateResources();
            CreateHolidays();
            await db.SaveChangesAsync();

            await CreateUsers();
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
                new Holiday { Date = new LocalDate(2020, 12, 31) }
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
    }
}