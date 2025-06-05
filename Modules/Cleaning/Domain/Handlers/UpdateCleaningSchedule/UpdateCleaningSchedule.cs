using Frederikskaj2.Reservations.Orders;
using static Frederikskaj2.Reservations.Cleaning.CleaningFunctions;

namespace Frederikskaj2.Reservations.Cleaning;

static class UpdateCleaningSchedule
{
    public static UpdateCleaningScheduleOutput UpdateCleaningScheduleCore(OrderingOptions options, UpdateCleaningScheduleInput input) =>
        new(CreateCleaningSchedule(options, input.Command.StartDate, input.Orders));
}
