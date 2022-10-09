using Frederikskaj2.Reservations.Shared.Core;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

record CleaningTasks(IEnumerable<CleaningTask> Tasks);
