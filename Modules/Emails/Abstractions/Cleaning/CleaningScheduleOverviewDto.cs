using Frederikskaj2.Reservations.LockBox;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Emails;

public record CleaningScheduleOverviewDto(CleaningScheduleDto Schedule, CleaningTasksDeltaDto Delta, IEnumerable<Resource> Resources);
