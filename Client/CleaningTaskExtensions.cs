using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Client;

static class CleaningTaskExtensions
{
    public static IEnumerable<CleaningDayInterval> ToIntervals(this CleaningTask task)
    {
        var begin = task.Begin.Date;
        var days = (task.End.Date - begin).Days + 1;
        for (var day = 0; day < days; day += 1)
            yield return new CleaningDayInterval
            {
                Date = begin.PlusDays(day),
                IsFirstDay = day == 0,
                IsLastDay = day == days - 1
            };
    }
}
