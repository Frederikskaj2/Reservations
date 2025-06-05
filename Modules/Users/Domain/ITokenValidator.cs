using NodaTime;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.Users;

public interface ITokenValidator
{
    TokenValidationResult ValidateConfirmEmailToken(Instant timestamp, UserId userId, ImmutableArray<byte> token);
    TokenValidationResult ValidateNewPasswordToken(Instant timestamp, EmailAddress email, ImmutableArray<byte> token);
}
