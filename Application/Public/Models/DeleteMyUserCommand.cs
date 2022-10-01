using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record DeleteMyUserCommand(Instant Timestamp, UserId UserId);
