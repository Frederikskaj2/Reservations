using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;
using CleaningTask = Frederikskaj2.Reservations.Server.Data.CleaningTask;
using Resource = Frederikskaj2.Reservations.Server.Data.Resource;
using User = Frederikskaj2.Reservations.Server.Data.User;

namespace Frederikskaj2.Reservations.Tests
{
    public class CleaningTaskServiceTests : IntegrationTests
    {
        [Fact]
        public async Task WhenAReservationIsInThePastExpectItSilentlyRemovedFromCleaningSchedule()
        {
            var fakeBackgroundWorkerQueue = new FakeBackgroundWorkQueue();
            await Initialize((serviceProvider, _) => serviceProvider.AddSingleton<IBackgroundWorkQueue<IEmailService>>(fakeBackgroundWorkerQueue));

            var reservationDate = new LocalDate(2020, 6, 1);

            await ModifyDatabase(db =>
            {
                var order = OrderBuilder
                    .CreaterOrder()
                    .WithUserId(1)
                    .WithFlags(OrderFlags.IsCleaningRequired)
                    .WithApartmentId(1)
                    .WithReservation(
                        builder => builder
                            .WithResourceId(1)
                            // Create it as settled as this was what triggered the problem.
                            .WithStatus(ReservationStatus.Settled)
                            .WithDateAndDuration(reservationDate, 1))
                    .WithReservation(
                        builder => builder
                            .WithResourceId(1)
                            .WithStatus(ReservationStatus.Confirmed)
                            .WithDateAndDuration(reservationDate.PlusWeeks(1), 1))
                    .Build();
                db.Orders.Add(order);
            });

            using (var scope = CreateScope())
            {
                var cleaningTaskService = scope.ServiceProvider.GetRequiredService<CleaningTaskService>();
                var aDayBeforeReservation = reservationDate.PlusDays(-1);
                await cleaningTaskService.SendDifferentialCleaningTasksEmail(aDayBeforeReservation);
            }

            using (var scope = CreateScope())
            {
                var cleaningTaskService = scope.ServiceProvider.GetRequiredService<CleaningTaskService>();
                var theDayTheReservationEnds = reservationDate.PlusDays(1);
                await cleaningTaskService.SendDifferentialCleaningTasksEmail(theDayTheReservationEnds);
            }

            await ModifyDatabase(db =>
            {
                var order = OrderBuilder
                    .CreaterOrder()
                    .WithUserId(1)
                    .WithFlags(OrderFlags.IsCleaningRequired)
                    .WithApartmentId(1)
                    .WithReservation(
                        builder => builder
                            .WithResourceId(1)
                            .WithStatus(ReservationStatus.Confirmed)
                            .WithDateAndDuration(reservationDate.PlusWeeks(2), 1))
                    .Build();
                db.Orders.Add(order);
            });

            using (var scope = CreateScope())
            {
                var cleaningTaskService = scope.ServiceProvider.GetRequiredService<CleaningTaskService>();
                var fourDaysAfterReservation = reservationDate.PlusDays(4);
                await cleaningTaskService.SendDifferentialCleaningTasksEmail(fourDaysAfterReservation);
            }

            Assert.Collection(
                fakeBackgroundWorkerQueue.Fakes,
                fake => A.CallTo(
                    () => fake.SendCleaningScheduleEmail(
                        A<User>._,
                        A<IEnumerable<Resource>>._,
                        A<IEnumerable<CleaningTask>>.That.IsEmpty(),
                        A<IEnumerable<CleaningTask>>.That.Matches(newTasks => newTasks.Count() == 2),
                        A<IEnumerable<CleaningTask>>.That.Matches(requiredTasks => requiredTasks.Count() == 2)))
                    .MustHaveHappenedOnceExactly(),
                fake => A.CallTo(
                    () => fake.SendCleaningScheduleEmail(
                        A<User>._,
                        A<IEnumerable<Resource>>._,
                        A<IEnumerable<CleaningTask>>.That.IsEmpty(),
                        A<IEnumerable<CleaningTask>>.That.Matches(requiredTasks => requiredTasks.Count() == 1),
                        A<IEnumerable<CleaningTask>>.That.Matches(requiredTasks => requiredTasks.Count() == 2)))
                    .MustHaveHappenedOnceExactly());
        }

        private class FakeBackgroundWorkQueue : IBackgroundWorkQueue<IEmailService>
        {
            private readonly List<IEmailService> fakes = new List<IEmailService>();

            public IEnumerable<IEmailService> Fakes => fakes;

            Task<Func<IEmailService, CancellationToken, Task>> IBackgroundWorkQueue<IEmailService>.Dequeue(CancellationToken cancellationToken)
                => Task.FromResult<Func<IEmailService, CancellationToken, Task>>(null);

            void IBackgroundWorkQueue<IEmailService>.Enqueue(Func<IEmailService, CancellationToken, Task> asyncAction)
            {
                var fake = A.Fake<IEmailService>();
                asyncAction(fake, CancellationToken.None).GetAwaiter().GetResult();
                fakes.Add(fake);
            }
        }
    }
}
