﻿using System.Collections.Generic;
using System.Threading.Tasks;
using NodaTime;

namespace Frederikskaj2.Reservations.Shared
{
    public interface IDataProvider
    {
        Task<IReadOnlyDictionary<int, Resource>> GetResources();
        bool IsHighPriceDay(LocalDate date);
        int GetNumberOfHighPriceDays(LocalDate date, int durationInDays);
        Task<IEnumerable<ReservedDay>> GetReservedDays(int resourceId, LocalDate fromDate, LocalDate toDate, bool includeOrder);
    }
}