using Frederikskaj2.Reservations.Core;
using NodaTime;
using System;

namespace Frederikskaj2.Reservations.RoomAccess;

class SendRoomEntryCodesJobRegistration(ITimeConverter timeConverter) : IJobRegistration
{
    public JobName Name => JobName.SendRoomEntryCodes;
    public IJobSchedule Schedule => new DailyJobSchedule(timeConverter, new(12, 0), Duration.FromMinutes(10));
    public Type JobType => typeof(SendRoomEntryCodesJob);
}
