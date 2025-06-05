using NodaTime;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.Users;

public record NewPasswordCommand(Instant Timestamp, EmailAddress Email, string NewPassword, ImmutableArray<byte> Token);
