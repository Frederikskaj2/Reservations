using NodaTime;
using NodaTime.Text;
using System;
using System.Text.Json.Serialization;

namespace Frederikskaj2.Reservations.Shared.Core;

public readonly struct Extent : IEquatable<Extent>
{
    static readonly LocalDatePattern pattern = LocalDatePattern.CreateWithInvariantCulture("yyyy-MM-dd");

    [JsonConstructor]
    public Extent(LocalDate date, int nights)
    {
        if (nights < 0)
            throw new ArgumentOutOfRangeException(nameof(nights));
        Date = date;
        Nights = nights;
    }

    public LocalDate Date { get; }

    public int Nights { get; }

    public bool Equals(Extent other) => Date == other.Date && Nights == other.Nights;

    public override bool Equals(object? obj) => obj is Extent period && Equals(period);

    public override int GetHashCode() => HashCode.Combine(Date, Nights);

    public LocalDate Ends() => Date.PlusDays(Nights);

    public bool Contains(LocalDate date) => Date <= date && date < Ends();

    public bool Overlaps(Extent other) => Date <= other.Date ? other.Date < Ends() : Date < other.Ends();

    public override string ToString() => $"{pattern.Format(Date)}+{Nights}";

    public static bool operator ==(Extent period1, Extent period2) => period1.Equals(period2);

    public static bool operator !=(Extent period1, Extent period2) => !(period1 == period2);
}
