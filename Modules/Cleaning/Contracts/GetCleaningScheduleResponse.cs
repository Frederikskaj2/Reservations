using Frederikskaj2.Reservations.Orders;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Cleaning;

public record GetCleaningScheduleResponse(IEnumerable<CleaningTask> CleaningTasks, IEnumerable<ReservedDay> ReservedDays);
