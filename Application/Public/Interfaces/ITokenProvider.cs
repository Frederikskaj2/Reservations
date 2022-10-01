using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.Application;

public interface ITokenProvider
{
    ImmutableArray<byte> GetConfirmEmailToken(Instant timestamp, UserId userId);
    TokenValidationResult ValidateConfirmEmailToken(Instant timestamp, UserId userId, ImmutableArray<byte> token);
    ImmutableArray<byte> GetNewPasswordToken(Instant timestamp, EmailAddress email);
    TokenValidationResult ValidateNewPasswordToken(Instant timestamp, EmailAddress email, ImmutableArray<byte> token);
}
