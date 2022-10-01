using NodaTime;

namespace Frederikskaj2.Reservations.Client;

public class CleaningDayInterval
{
    public LocalDate Date { get; init; }
    public bool IsFirstDay { get; init; }
    public bool IsLastDay { get; init; }
}