using LanguageExt;
using System;
using System.Collections.Generic;
using System.Linq;
using Frederikskaj2.Reservations.Shared.Core;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application;

public static class Apartments
{
    public static Func<ApartmentId, bool> IsValid =>
        apartmentId => GetDictionary().ContainsKey(apartmentId);

    public static Func<IEnumerable<Apartment>> GetAll { get; } =
        memo(() => GetApartments().ToList());

    public static Func<ApartmentId, Option<Apartment>> GetApartment =>
        apartmentId => GetDictionary().TryGetValue(apartmentId, out var apartment) ? Some(apartment) : None;

    static Func<Dictionary<ApartmentId, Apartment>> GetDictionary { get; } =
        memo(() => GetAll().ToDictionary(apartment => apartment.ApartmentId));

    static IEnumerable<Apartment> GetApartments()
    {
        var stories = new Dictionary<char, int>
        {
            { 'A', 5 },
            { 'B', 4 },
            { 'C', 5 },
            { 'D', 3 },
            { 'E', 5 },
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
        var apartments = stories.SelectMany(
                kvp => Enumerable.Range(0, kvp.Value + 1)
                    .SelectMany(story => new[]
                    {
                        new ApartmentModel(kvp.Key, story, ApartmentSide.Left),
                        new ApartmentModel(kvp.Key, story, ApartmentSide.Right)
                    }))
            .Concat(new[]
            {
                new ApartmentModel('E', 6, default),
                new ApartmentModel('E', 7, default),
                new ApartmentModel('L', 6, default),
                new ApartmentModel('L', 7, default),
                new ApartmentModel('R', 7, default),
                new ApartmentModel('V', default, default),
                new ApartmentModel('W', default, default)
            })
            .OrderBy(apartment => apartment.Letter)
            .ThenBy(apartment => apartment.Story)
            .ThenBy(apartment => apartment.Side);
        yield return Apartment.Deleted;
        var id = 1;
        foreach (var apartment in apartments)
            yield return new Apartment(id++, apartment.Letter, apartment.Story, apartment.Side);
    }

    record ApartmentModel(char Letter, int? Story, ApartmentSide Side);
}
