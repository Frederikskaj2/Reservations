using NodaTime;

namespace Frederikskaj2.Reservations.Application;

public interface IFormatter
{
    string FormatDateShort(LocalDate date);
}