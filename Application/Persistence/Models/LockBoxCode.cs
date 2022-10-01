using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record LockBoxCode(ResourceId ResourceId, LocalDate Date, string Code);
