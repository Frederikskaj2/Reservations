using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Users;

[Primitive(Features.Default | Features.Comparable)]
public readonly partial struct PaymentId
{
    readonly string id;
}
