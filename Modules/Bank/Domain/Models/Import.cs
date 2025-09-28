using Frederikskaj2.Reservations.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public sealed record Import(Instant Timestamp, LocalDate StartDate) : IHasId
{
    public const string SingletonId = "";

    string IHasId.GetId() => SingletonId;
}
