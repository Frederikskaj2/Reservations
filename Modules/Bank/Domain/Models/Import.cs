using Frederikskaj2.Reservations.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record Import(LocalDate StartDate) : IHasId
{
    public const string SingletonId = "";

    public string GetId() => SingletonId;
}
