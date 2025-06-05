using Liversage.Primitives;

namespace Frederikskaj2.Reservations.LockBox;

[Primitive(Features.Equatable | Features.Comparable)]
public readonly partial struct ResourceId
{
    readonly int id;
}
