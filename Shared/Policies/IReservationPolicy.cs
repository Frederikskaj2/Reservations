using System.Threading.Tasks;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public interface IReservationPolicy
    {
        ResourceType Type { get; }
        Task<(int MinimumDays, int MaximumDays)> GetReservationAllowedNumberOfDays(
            int resourceId, LocalDate date, bool includeOrder = true);
        Task<bool> IsResourceAvailable(LocalDate date, int resourceId, bool includeOrder = true);
        Price GetPrice(LocalDate date, int durationInDays);
    }
}