using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public interface IDateProvider
    {
        LocalDate Today { get; }
    }
}