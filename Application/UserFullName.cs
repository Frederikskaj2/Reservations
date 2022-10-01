using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

class UserFullName
{
    public UserId UserId { get; init; }
    public string FullName { get; init; } = null!;
}
