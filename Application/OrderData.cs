using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record OrderData(IFormatter Formatter, OrderingOptions Options, Instant Timestamp, LocalDate Today, UserId UserId, OrderId OrderId);
