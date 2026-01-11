using Frederikskaj2.Reservations.Core;
using Frederikskaj2.Reservations.Users;

namespace Frederikskaj2.Reservations.Bank;

public sealed record InProgressPayOut(UserId UserId, PayOutId PayOutId) : IHasId
{
    string IHasId.GetId() => UserId.GetId();
}
