using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Shared.Core;

[Primitive(Features.Default | Features.Comparable)]
public readonly partial struct TransactionId
{
    readonly int id;

    public static TransactionId Add(TransactionId id, int value) => FromInt32(id.ToInt32() + value);

    public static TransactionId operator +(TransactionId id, int value) => Add(id, value);
}
