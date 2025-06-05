namespace Frederikskaj2.Reservations.Cleaning;

public record CleaningInterval(CleaningTask Task, bool IsFirstDay, bool IsLastDay);
