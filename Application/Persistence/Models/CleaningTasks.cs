using Frederikskaj2.Reservations.Shared.Core;
using NodaTime;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

record CleaningTasks(Instant Timestamp, IEnumerable<CleaningTask> Tasks);
