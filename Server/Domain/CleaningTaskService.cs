using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Frederikskaj2.Reservations.Server.Data;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using CleaningTask = Frederikskaj2.Reservations.Server.Data.CleaningTask;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Server.Domain
{
    public class CleaningTaskService
    {
        private static readonly IEqualityComparer<CleaningTask> EqualityComparer = new CleaningTaskEqualityComparer();
        private readonly IBackgroundWorkQueue<EmailService> backgroundWorkQueue;
        private readonly ReservationsContext db;

        public CleaningTaskService(
            IBackgroundWorkQueue<EmailService> backgroundWorkQueue, ReservationsContext db)
        {
            this.backgroundWorkQueue =
                backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<IEnumerable<CleaningTask>> GetTasks(LocalDate date)
        {
            var confirmedReservations = await db.Reservations
                .Where(reservation => reservation.Status == ReservationStatus.Confirmed && reservation.Order!.Flags.HasFlag(OrderFlags.IsCleaningRequired)).ToListAsync();
            return confirmedReservations
                .Where(reservation => date <= reservation.Date.PlusDays(reservation.DurationInDays))
                .Select(
                    reservation => new CleaningTask
                    {
                        ResourceId = reservation.ResourceId,
                        Date = reservation.Date.PlusDays(reservation.DurationInDays)
                    });
        }

        public async Task SendCleaningTasksEmail(User user, LocalDate date)
        {
            if (user is null)
                throw new ArgumentNullException(nameof(user));

            var requiredTasks = await GetTasks(date);
            var resources = await db.Resources.ToListAsync();
            backgroundWorkQueue.Enqueue(
                (service, _) => service.SendCleaningScheduleEmail(
                    user, resources, Enumerable.Empty<CleaningTask>(), Enumerable.Empty<CleaningTask>(), requiredTasks));
        }

        public async Task SendDifferentialCleaningTasksEmail(LocalDate date)
        {
            var allTasks = await db.CleaningTasks.ToListAsync();
            var currentTasks = allTasks.Where(cleaningTask => cleaningTask.Date >= date).ToHashSet(EqualityComparer);
            var requiredTasks = (await GetTasks(date)).ToHashSet(EqualityComparer);

            var cancelledTasks = currentTasks.ToHashSet(EqualityComparer);
            cancelledTasks.ExceptWith(requiredTasks);

            var newTasks = requiredTasks.ToHashSet(EqualityComparer);
            newTasks.ExceptWith(currentTasks);

            if (cancelledTasks.Count != 0 || newTasks.Count != 0)
            {
                var resources = await db.Resources.ToListAsync();
                var users = await db.Users
                    .Where(user => user.EmailSubscriptions.HasFlag(EmailSubscriptions.CleaningScheduleUpdated))
                    .ToListAsync();
                foreach (var user in users)
                    backgroundWorkQueue.Enqueue(
                        (service, _) => service.SendCleaningScheduleEmail(
                            user, resources, cancelledTasks, newTasks, requiredTasks));
            }

            foreach (var task in allTasks.Where(task => !requiredTasks.Contains(task)))
                db.CleaningTasks.Remove(task);
            foreach (var task in newTasks)
                db.CleaningTasks.Add(task);

            await db.SaveChangesAsync();
        }

        private class CleaningTaskEqualityComparer : IEqualityComparer<CleaningTask>
        {
            public bool Equals(CleaningTask? x, CleaningTask? y)
            {
                if (ReferenceEquals(x, y))
                    return true;
                if (x == null || y == null)
                    return false;
                return x.ResourceId == y.ResourceId && x.Date == y.Date;
            }

            public int GetHashCode(CleaningTask obj) => HashCode.Combine(obj.ResourceId, obj.Date);
        }
    }
}