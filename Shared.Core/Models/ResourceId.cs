using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Shared.Core;

[Primitive(Features.Equatable | Features.Comparable)]
public readonly partial struct ResourceId
{
    readonly int id;
}
