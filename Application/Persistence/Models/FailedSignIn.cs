using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record FailedSignIn(Instant Timestamp, int Count);