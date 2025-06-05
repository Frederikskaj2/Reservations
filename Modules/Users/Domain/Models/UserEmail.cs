using Frederikskaj2.Reservations.Core;

namespace Frederikskaj2.Reservations.Users;

public record UserEmail(string NormalizedEmail, UserId UserId) : IHasId
{
    public string GetId() => NormalizedEmail;
}
