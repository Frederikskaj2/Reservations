using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public interface IDataProvider
    {
        Task<bool> IsHighPriceDay(LocalDate date);
        Task<int> GetNumberOfHighPriceDays(Reservation reservation);
        Task<IEnumerable<Reservation>> GetReservations(int resourceId, LocalDate fromDate, LocalDate toDate);
        void Refresh();
    }
}