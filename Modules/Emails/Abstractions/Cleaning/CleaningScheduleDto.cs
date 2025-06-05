using Frederikskaj2.Reservations.Cleaning;
using Frederikskaj2.Reservations.Orders;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Emails;

public record CleaningScheduleDto(IEnumerable<CleaningTask> CleaningTasks, IEnumerable<ReservedDay> ReservedDays);
