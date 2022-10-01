using System.Collections.Immutable;
using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record NewPasswordCommand(Instant Timestamp, EmailAddress Email, string NewPassword, ImmutableArray<byte> Token);
