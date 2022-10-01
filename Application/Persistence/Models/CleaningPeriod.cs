using NodaTime;

namespace Frederikskaj2.Reservations.Application;

record CleaningPeriod(LocalDateTime Begin, LocalDateTime End);