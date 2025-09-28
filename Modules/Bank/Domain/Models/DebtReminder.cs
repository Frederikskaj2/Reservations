using Frederikskaj2.Reservations.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public sealed record DebtReminder(Instant LatestExecutionTime) : IHasId
{
    public const string SingletonId = "";

    string IHasId.GetId() => SingletonId;
}
