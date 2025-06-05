using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;
using NodaTime;

namespace Frederikskaj2.Reservations.Bank;

public record PayOut(PayOutId PayOutId, Instant Timestamp, UserId UserId, Amount Amount) : IHasId
{
    public string GetId() => PayOutId.GetId();
}
