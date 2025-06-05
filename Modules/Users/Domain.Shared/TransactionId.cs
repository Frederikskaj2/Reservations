using Frederikskaj2.Reservations.Core;
using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Users;

[Primitive(Features.Default | Features.Comparable)]
public readonly partial struct TransactionId : IIsId
{
    readonly int id;

    public string GetId() => ToString();

    public static TransactionId Add(TransactionId id, int value) => FromInt32(id.ToInt32() + value);

    public static TransactionId operator +(TransactionId id, int value) => Add(id, value);
}
