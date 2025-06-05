using Duration = NodaTime.Duration;

namespace Frederikskaj2.Reservations.Users;

public record Cookie(string Name, string Value, Duration? MaxAge);
