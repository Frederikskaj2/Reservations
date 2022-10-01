using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Shared.Core;

[Primitive(Features.Default | Features.Comparable)]
public readonly partial struct PaymentId
{
    readonly string id;
}
