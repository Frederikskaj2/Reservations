using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record UpdatePasswordCommand(Instant Timestamp, string CurrentPassword, string NewPassword, ParsedRefreshToken ParsedToken);