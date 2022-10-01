using NodaTime;

namespace Frederikskaj2.Reservations.Server;

public record Cookie(string Name, string Value, Duration? MaxAge);
