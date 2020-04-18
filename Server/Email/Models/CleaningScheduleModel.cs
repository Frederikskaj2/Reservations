using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Server.Email
{
    public class CleaningScheduleModel : EmailWithNameModel
    {
        public CleaningScheduleModel(
            string from, Uri fromUri, string name, IEnumerable<CleaningTask> cancelledTasks,
            IEnumerable<CleaningTask> newTasks, IEnumerable<CleaningTask> currentTasks) : base(from, fromUri, name)
        {
            CancelledTasks = cancelledTasks ?? throw new ArgumentNullException(nameof(cancelledTasks));
            NewTasks = newTasks ?? throw new ArgumentNullException(nameof(newTasks));
            CurrentTasks = currentTasks ?? throw new ArgumentNullException(nameof(currentTasks));
        }

        public IEnumerable<CleaningTask> CancelledTasks { get; }
        public IEnumerable<CleaningTask> NewTasks { get; }
        public IEnumerable<CleaningTask> CurrentTasks { get; }
    }
}