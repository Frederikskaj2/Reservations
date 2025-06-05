using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Users;

[method: SetsRequiredMembers]
public record UserFullName(UserId UserId, string FullName);
