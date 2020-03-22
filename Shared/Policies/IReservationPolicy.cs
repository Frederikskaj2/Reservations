using System.Threading.Tasks;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public interface IReservationPolicy
    {
        ResourceType Type { get; }
        Task<(int MinimumDays, int MaximumDays)> GetReservationAllowedNumberOfDays(int resourceId, LocalDate date);
        Task<bool> IsResourceAvailable(LocalDate date, int resourceId);
        Task<bool> IsResourceAvailable(LocalDate date, int durationInDays, int resourceId);
        Task<Price> GetPrice(LocalDate date, int durationInDays);
    }
}