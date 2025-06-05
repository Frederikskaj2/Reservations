using NodaTime;
using System;

namespace Frederikskaj2.Reservations.Emails;

public record ConfirmEmailDto(Uri Url, Duration ConfirmEmailDuration);
