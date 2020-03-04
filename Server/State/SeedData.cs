using System;
using System.Collections.Generic;
using System.Linq;
using Frederikskaj2.Reservations.Shared;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.State
{
    internal static class SeedData
    {
        public static void Initialize(ReservationsContext db)
        {
            var resources = new[]
            {
                new Resource { Sequence = 1, Name = "Fest-/aktivitetslokale" },
                new Resource { Sequence = 2, Name = "Frederikke (soveværelse)" },
                new Resource { Sequence = 3, Name = "Kaj (soveværelse)" }
            };
            db.Resources.AddRange(resources);

            var apartments = GetApartments().ToList();
            db.Apartments.AddRange(apartments);

            var random = new Random();
            var timeZoneInfo = DateTimeZoneProviders.Tzdb.GetZoneOrNull("Europe/Copenhagen")!;
            var resourceReservations = new Dictionary<(LocalDate, Resource), ResourceReservation>();
            while (resourceReservations.Count < 40)
            {
                var resource = resources[random.Next(resources.Length)];
                var date = SystemClock.Instance.GetCurrentInstant().InZone(timeZoneInfo).Date.PlusDays(random.Next(60));
                var key = (date, resource);
                if (resourceReservations.ContainsKey(key))
                    continue;
                var resourceReservation = new ResourceReservation
                {
                    Date = date,
                    DurationInDays = 1,
                    Resource = resource,
                    Status = ReservationStatus.Reserved
                };
                resourceReservations.Add(key, resourceReservation);
            }
            db.ResourceReservations.AddRange(resourceReservations.Values);

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
                            .Select(story => new Apartment { Letter = kvp.Key, Story = story, Side = Side.Left })
                            .Concat(Enumerable.Range(0, kvp.Value.Right + 1)
                                .Select(story => new Apartment { Letter = kvp.Key, Story = story, Side = Side.Right })))
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