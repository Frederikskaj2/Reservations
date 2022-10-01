using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public record NoFeeCancellationAllowedModel(EmailAddress Email, string FullName, OrderId OrderId, Period Duration);
