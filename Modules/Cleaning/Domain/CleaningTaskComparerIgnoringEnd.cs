using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Cleaning;

class CleaningTaskComparerIgnoringEnd : IEqualityComparer<CleaningTask>
{
    public static readonly CleaningTaskComparerIgnoringEnd Instance = new();

    public bool Equals(CleaningTask? x, CleaningTask? y) => x?.Begin == y?.Begin && x?.ResourceId == y?.ResourceId;

    public int GetHashCode(CleaningTask obj) => HashCode.Combine(obj.Begin.GetHashCode(), obj.ResourceId.GetHashCode());
}
