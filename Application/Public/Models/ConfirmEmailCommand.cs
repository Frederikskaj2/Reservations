using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Immutable;

namespace Frederikskaj2.Reservations.Application;

public record ConfirmEmailCommand(Instant Timestamp, EmailAddress Email, ImmutableArray<byte> Token);
