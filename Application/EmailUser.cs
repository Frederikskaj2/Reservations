using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Application;

public class EmailUser
{
    public UserId UserId { get; init; }
    public EmailAddress Email { get; init; }
    public string FullName { get; init; } = null!;
};
