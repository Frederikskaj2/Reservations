using NodaTime;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.Users;

public record ConfirmEmailCommand(Instant Timestamp, EmailAddress Email, ImmutableArray<byte> Token);
