using Frederikskaj2.Reservations.Shared.Core;
using System;
using System.Collections.Generic;

namespace Frederikskaj2.Reservations.Application;

class CleaningTaskComparer : IEqualityComparer<CleaningTask>
{
    public static readonly CleaningTaskComparer Instance = new();

    public bool Equals(CleaningTask? x, CleaningTask? y) => x?.Begin == y?.Begin && x?.ResourceId == y?.ResourceId;

    public int GetHashCode(CleaningTask obj) => HashCode.Combine(obj.Begin.GetHashCode(), obj.ResourceId.GetHashCode());
}
