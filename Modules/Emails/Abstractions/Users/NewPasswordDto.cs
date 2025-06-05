using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Emails;

public record NewPasswordDto(Uri Url, Duration NewPasswordDuration);
