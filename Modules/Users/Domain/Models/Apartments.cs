using LanguageExt;
using System.Collections.Generic;
using System.Linq;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Users;

public static class Apartments
{
    public static Seq<Apartment> All { get; } = GetApartments().ToSeq();

    static readonly HashMap<ApartmentId, Apartment> hashMap = toHashMap(All.Map(apartment => (apartment.ApartmentId, apartment)));

    public static bool IsValid(ApartmentId apartmentId) => hashMap.ContainsKey(apartmentId);

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
            { 'R', 6 },
        };
        var apartments = stories.SelectMany(
                kvp => Enumerable.Range(0, kvp.Value + 1)
                    .SelectMany(story => new[]
                    {
                        new ApartmentModel(kvp.Key, story, ApartmentSide.Left),
                        new ApartmentModel(kvp.Key, story, ApartmentSide.Right),
                    }))
            .Concat([
                new('E', 6, default),
                new('E', 7, default),
                new('L', 6, default),
                new('L', 7, default),
                new('R', 7, default),
                new('V', null, default),
                new('W', null, default),
            ])
            .OrderBy(apartment => apartment.Letter)
            .ThenBy(apartment => apartment.Story)
            .ThenBy(apartment => apartment.Side);
        yield return Apartment.Deleted;
        var id = 1;
        foreach (var apartment in apartments)
            yield return new(id++, apartment.Letter, apartment.Story, apartment.Side);
    }

    record ApartmentModel(char Letter, int? Story, ApartmentSide Side);
}
