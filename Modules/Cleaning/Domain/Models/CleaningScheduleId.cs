using Frederikskaj2.Reservations.Core;
using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Cleaning;

[Primitive]
public readonly partial struct CleaningScheduleId : IIsId
{
    readonly string id;

    public string GetId() => ToString();
}
