using Frederikskaj2.Reservations.Cleaning;
using Frederikskaj2.Reservations.Orders;
using Frederikskaj2.Reservations.Server.IntegrationTests.Harness;
using LightBDD.Framework;
using LightBDD.XUnit2;
using NodaTime;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Frederikskaj2.Reservations.Server.IntegrationTests.Tests.Cleaning;

public abstract class CleaningFixture(SessionFixture session) : FeatureFixture, IClassFixture<SessionFixture>
{
    protected const int AdditionalDaysWhereCleaningCanBeDone = 3;
    protected const int IntervalInDays = 1;

    static readonly LocalTime checkoutTime = new(10, 0);

    protected static readonly LocalTime CheckinTime = new(12, 0);

    State<IEnumerable<CleaningTask>> cleaningTasks;

    protected SessionFixture Session { get; } = session;

    protected IEnumerable<CleaningTask> CleaningTasks => cleaningTasks.GetValue(nameof(CleaningTasks));

    protected async Task WhenTheCleaningScheduleIsRetrieved()
    {
        await Session.UpdateCleaningSchedule();
        var getCleaningScheduleResponse = await Session.GetCleaningSchedule();
        cleaningTasks = new(getCleaningScheduleResponse.CleaningTasks);
    }

    protected static CleaningTask? GetCleaningTaskForReservation(IEnumerable<CleaningTask> cleaningTasks, ReservationDto reservation) =>
        cleaningTasks.FirstOrDefault(task => task.ResourceId == reservation.ResourceId && task.Begin == reservation.Extent.Ends().At(checkoutTime));
}
