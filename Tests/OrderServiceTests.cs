using System;
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
    public class OrderServiceTests : IntegrationTests
    {
        [Fact]
        public async Task WhenReservationEndsExpectSettlementEmailToBeSent()
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
                    .WithReservation(
                        builder => builder
                            .WithResourceId(1)
                            .WithStatus(ReservationStatus.Confirmed)
                            .WithDateAndDuration(reservationDate, 1))
                    .Build();
                db.Orders.Add(order);
            });

            using var scope = CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

            var aDayAfterReservation = reservationDate.PlusDays(1);
            await orderService.SendSettlementNeededEmails(aDayAfterReservation);
            A.CallTo(() => backgroundWorkerQueue.Enqueue(A<Func<IEmailService, CancellationToken, Task>>.Ignored)).MustHaveHappened();
        }

        [Fact]
        public async Task WhenOwnerReservationEndsExpectNoSettlementEmailToBeSent()
        {
            var backgroundWorkerQueue = A.Fake<IBackgroundWorkQueue<IEmailService>>();
            await Initialize((serviceProvider, _) => serviceProvider.AddSingleton(backgroundWorkerQueue));

            var reservationDate = new LocalDate(2020, 6, 1);

            await ModifyDatabase(db =>
            {
                var order = OrderBuilder
                    .CreaterOrder()
                    .WithUserId(1)
                    .WithFlags(OrderFlags.IsOwnerOrder)
                    .WithReservation(
                        builder => builder
                            .WithResourceId(1)
                            .WithStatus(ReservationStatus.Confirmed)
                            .WithDateAndDuration(reservationDate, 1))
                    .Build();
                db.Orders.Add(order);
            });

            using var scope = CreateScope();
            var orderService = scope.ServiceProvider.GetRequiredService<OrderService>();

            var aDayAfterReservation = reservationDate.PlusDays(1);
            await orderService.SendSettlementNeededEmails(aDayAfterReservation);
            A.CallTo(() => backgroundWorkerQueue.Enqueue(A<Func<IEmailService, CancellationToken, Task>>.Ignored)).MustNotHaveHappened();
        }
    }
}
