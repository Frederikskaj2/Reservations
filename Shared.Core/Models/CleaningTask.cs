using NodaTime;

namespace Frederikskaj2.Reservations.Shared.Core;

public record CleaningTask(ResourceId ResourceId, LocalDateTime Begin, LocalDateTime End);
