using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record FailedSignIn(Instant Timestamp, int Count);
