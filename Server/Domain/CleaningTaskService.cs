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

namespace Frederikskaj2.Reservations.Server.Domain
{
    public class CleaningTaskService
    {
        private static readonly IEqualityComparer<CleaningTask> EqualityComparer = new CleaningTaskEqualityComparer();
        private readonly IBackgroundWorkQueue<EmailService> backgroundWorkQueue;
        private readonly ReservationsContext db;
        private readonly KeyCodeService keyCodeService;

        public CleaningTaskService(
            IBackgroundWorkQueue<EmailService> backgroundWorkQueue, ReservationsContext db,
            KeyCodeService keyCodeService)
        {
            this.backgroundWorkQueue =
                backgroundWorkQueue ?? throw new ArgumentNullException(nameof(backgroundWorkQueue));
            this.db = db ?? throw new ArgumentNullException(nameof(db));
            this.keyCodeService = keyCodeService ?? throw new ArgumentNullException(nameof(keyCodeService));
        }

        public async Task SendCleaningTasksEmail(LocalDate date)
        {
            var keyCodes = await keyCodeService.GetKeyCodes(date);
            var mostFutureKeyCodeDate = keyCodes.OrderByDescending(keyCode => keyCode.Date).First().Date;

            var allTasks = await db.CleaningTasks.ToListAsync();
            var currentTasks = allTasks.Where(cleaningTask => cleaningTask.Date >= date).ToHashSet(EqualityComparer);
            var confirmedReservations = await db.Reservations
                .Where(reservation => reservation.Status == ReservationStatus.Confirmed).ToListAsync();
            var requiredTasks = confirmedReservations
                .Where(
                    reservation => date <= reservation.Date.PlusDays(reservation.DurationInDays)
                        && reservation.Date.PlusDays(reservation.DurationInDays) <= mostFutureKeyCodeDate.PlusDays(6))
                .Select(
                    reservation => new CleaningTask
                    {
                        ResourceId = reservation.ResourceId,
                        Date = reservation.Date.PlusDays(reservation.DurationInDays)
                    })
                .ToHashSet(EqualityComparer);

            var cancelledTasks = currentTasks.ToHashSet(EqualityComparer);
            cancelledTasks.ExceptWith(requiredTasks);

            var newTasks = requiredTasks.ToHashSet(EqualityComparer);
            newTasks.ExceptWith(currentTasks);

            if (cancelledTasks.Count != 0 || newTasks.Count != 0)
            {
                var resources = await db.Resources.ToListAsync();
                backgroundWorkQueue.Enqueue(
                    (service, _) => service.SendCleaningScheduleEmail(
                        resources, keyCodes, cancelledTasks, newTasks, requiredTasks));
            }

            foreach (var task in allTasks.Where(task => !requiredTasks.Contains(task)))
                db.CleaningTasks.Remove(task);
            foreach (var task in newTasks)
                db.CleaningTasks.Add(task);

            await db.SaveChangesAsync();
        }

        private class CleaningTaskEqualityComparer : IEqualityComparer<CleaningTask>
        {
            public bool Equals(CleaningTask x, CleaningTask y)
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