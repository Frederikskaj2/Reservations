using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Users;

public record SignInCommand(Instant Timestamp, EmailAddress Email, string Password, bool IsPersistent, Option<DeviceId> DeviceId);
