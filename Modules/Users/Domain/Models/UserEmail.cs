using Frederikskaj2.Reservations.Core;

namespace Frederikskaj2.Reservations.Users;

public sealed record UserEmail(string NormalizedEmail, UserId UserId) : IHasId
{
    string IHasId.GetId() => NormalizedEmail;
}
