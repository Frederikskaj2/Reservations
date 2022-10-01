using Frederikskaj2.Reservations.Shared.Core;

namespace Frederikskaj2.Reservations.Client;

public record CleaningInterval(CleaningTask Task, bool IsFirstDay, bool IsLastDay);
