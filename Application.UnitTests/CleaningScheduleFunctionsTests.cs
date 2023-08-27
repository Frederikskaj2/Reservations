using FakeItEasy;
using FluentAssertions;
using Frederikskaj2.Reservations.Shared.Core;
using LanguageExt;
using NodaTime;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;
using static LanguageExt.Prelude;

namespace Frederikskaj2.Reservations.Application.UnitTests;

public class CleaningScheduleFunctionsTests
{
    [Fact]
    public async Task CleaningTaskInThePastShouldNotAppearAsCancelledTask()
    {
        // Arrange
        const int additionalDaysWhereCleaningCanBeDone = 3;
        var timestamp = SystemClock.Instance.GetCurrentInstant();
        var reservationStart = timestamp.InZone(DateTimeZone.Utc).Date.PlusDays(1);
        const int reservationNights = 1;
        var cleaningStart = reservationStart.PlusDays(reservationNights);
        var cleaningEnd = cleaningStart.PlusDays(additionalDaysWhereCleaningCanBeDone);
        var resourceId = ResourceId.FromInt32(1);
        var checkInTime = new LocalTime(12, 0);
        var checkOutTime = new LocalTime(10, 0);
        var reservation = new Reservation(
            resourceId,
            ReservationStatus.Confirmed,
            new(reservationStart, reservationNights),
            null,
            default,
            new(cleaningStart.At(checkOutTime), cleaningEnd.At(checkInTime)));
        var order = new Order(
            OrderId.FromInt32(1),
            UserId.FromInt32(1),
            OrderFlags.IsCleaningRequired,
            timestamp,
            new(ApartmentId.FromInt32(1), null, Empty),
            null,
            Seq1(reservation),
            Empty);
        var cleaningTask = new CleaningTask(
            resourceId,
            cleaningStart.At(checkOutTime),
            cleaningEnd.At(checkInTime));
        var cleaningTasks = new CleaningTasks(new[] { cleaningTask });
        const string partitionKey = "";
        const string singletonId = "";
        var persistenceContextFactory = A.Fake<IPersistenceContextFactory>();
        var emptyPersistenceContext = A.Fake<IPersistenceContext>();
        A.CallTo(() => persistenceContextFactory.Create(partitionKey)).Returns(emptyPersistenceContext);
        var persistenceContextWithCleaningTasks = A.Fake<IPersistenceContext>();
        A.CallTo(() => emptyPersistenceContext.ReadItem(singletonId, A<Func<CleaningTasks>>.Ignored))
            .Returns(EitherAsync<HttpStatusCode, IPersistenceContext>.Right(persistenceContextWithCleaningTasks));
        var untracked = A.Fake<IUntracked>();
        A.CallTo(() => persistenceContextWithCleaningTasks.Untracked).Returns(untracked);
        var orderQuery = A.Fake<IQuery<Order>>();
        A.CallTo(() => persistenceContextWithCleaningTasks.Query<Order>()).Returns(orderQuery);
        A.CallTo(() => untracked.ReadItems(An<IQuery<Order>>.Ignored)).Returns(RightAsync<HttpStatusCode, IEnumerable<Order>>(new[] { order }));
        A.CallTo(() => persistenceContextWithCleaningTasks.Item<CleaningTasks>()).Returns(cleaningTasks);
        var options = new OrderingOptions { AdditionalDaysWhereCleaningCanBeDone = additionalDaysWhereCleaningCanBeDone };

        // Act
        var either = CleaningScheduleFunctions.TryGetCleaningScheduleDelta(persistenceContextFactory, options, cleaningEnd.PlusDays(1));
        var (schedule, deltaOption) = await either.MatchAsync(
            tuple => tuple,
            failure => throw new XunitException(failure.ToString()));

        // Assert
        schedule.CleaningTasks.Should().BeEmpty();
        deltaOption.IsNone.Should().BeTrue();
    }
}
