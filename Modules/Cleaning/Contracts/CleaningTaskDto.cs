using Frederikskaj2.Reservations.LockBox;
using NodaTime;

namespace Frederikskaj2.Reservations.Cleaning;

public record CleaningTaskDto(ResourceId ResourceId, LocalDateTime Begin, LocalDateTime End);
