using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record SignInCommand(Instant Timestamp, EmailAddress Email, string Password, bool IsPersistent, Option<DeviceId> DeviceId);
