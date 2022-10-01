using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.EmailSender;

static class CleaningTaskExtensions
{
    public static IEnumerable<CleaningDayInterval> ToIntervals(this CleaningTask task)
    {
        var begin = task.Begin.Date;
        var days = (task.End.Date - begin).Days + 1;
        for (var day = 0; day < days; day += 1)
            yield return new CleaningDayInterval(begin.PlusDays(day), day == 0, day == days - 1);
    }
}
