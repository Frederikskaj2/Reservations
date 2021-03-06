﻿using System;
using System.Threading;
using System.Threading.Tasks;
using FakeItEasy;
using Frederikskaj2.Reservations.Server.Domain;
using Frederikskaj2.Reservations.Server.Email;
using Frederikskaj2.Reservations.Shared;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using Xunit;

namespace Frederikskaj2.Reservations.Tests
{
    public class LockBoxCodeServiceTests : IntegrationTests
    {
        [Fact]
        public async Task WhenAWeekBeforeReservationExpectNoLockBoxCodeEmailToBeSent()
        {
            var backgroundWorkerQueue = A.Fake<IBackgroundWorkQueue<IEmailService>>();
            await Initialize((serviceProvider, _) => serviceProvider.AddSingleton(backgroundWorkerQueue));

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
                            .WithStatus(ReservationStatus.Confirmed)
                            .WithDateAndDuration(reservationDate, 1))
                    .Build();
                db.Orders.Add(order);
            });

            using var scope = CreateScope();
            var lockBoxCodeService = scope.ServiceProvider.GetRequiredService<LockBoxCodeService>();

            var aWeekBeforeReservation = reservationDate.PlusWeeks(-1);
            await lockBoxCodeService.SendLockBoxCodeEmails(aWeekBeforeReservation);
            A.CallTo(() => backgroundWorkerQueue.Enqueue(A<Func<IEmailService, CancellationToken, Task>>.Ignored)).MustNotHaveHappened();
        }

        [Fact]
        public async Task WhenADayBeforeReservationExpectLockBoxCodeEmailToBeSent()
        {
            var backgroundWorkerQueue = A.Fake<IBackgroundWorkQueue<IEmailService>>();
            await Initialize((serviceProvider, _) => serviceProvider.AddSingleton(backgroundWorkerQueue));

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
                            .WithStatus(ReservationStatus.Confirmed)
                            .WithDateAndDuration(reservationDate, 1))
                    .Build();
                db.Orders.Add(order);
            });

            using var scope = CreateScope();
            var lockBoxCodeService = scope.ServiceProvider.GetRequiredService<LockBoxCodeService>();

            var aDayBeforeReservation = reservationDate.PlusDays(-1);
            await lockBoxCodeService.SendLockBoxCodeEmails(aDayBeforeReservation);
            A.CallTo(() => backgroundWorkerQueue.Enqueue(A<Func<IEmailService, CancellationToken, Task>>.Ignored)).MustHaveHappenedOnceExactly();
        }

        [Fact]
        public async Task WhenADayBeforeOwnerReservationExpectNoLockBoxCodeEmailToBeSent()
        {
            var backgroundWorkerQueue = A.Fake<IBackgroundWorkQueue<IEmailService>>();
            await Initialize((serviceProvider, _) => serviceProvider.AddSingleton(backgroundWorkerQueue));

            var reservationDate = new LocalDate(2020, 6, 1);

            await ModifyDatabase(db =>
            {
                var order = OrderBuilder
                    .CreaterOrder()
                    .WithUserId(1)
                    .WithFlags(OrderFlags.IsCleaningRequired | OrderFlags.IsOwnerOrder)
                    .WithApartmentId(1)
                    .WithReservation(
                        builder => builder
                            .WithResourceId(1)
                            .WithStatus(ReservationStatus.Confirmed)
                            .WithDateAndDuration(reservationDate, 1))
                    .Build();
                db.Orders.Add(order);
            });

            using var scope = CreateScope();
            var lockBoxCodeService = scope.ServiceProvider.GetRequiredService<LockBoxCodeService>();

            var aDayBeforeReservation = reservationDate.PlusDays(-1);
            await lockBoxCodeService.SendLockBoxCodeEmails(aDayBeforeReservation);
            A.CallTo(() => backgroundWorkerQueue.Enqueue(A<Func<IEmailService, CancellationToken, Task>>.Ignored)).MustNotHaveHappened();
        }
    }
}
