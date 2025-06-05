using Frederikskaj2.Reservations.Core;
using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Bank;

[Primitive(Features.Equatable | Features.Formattable)]
public readonly partial struct BankTransactionId : IIsId
{
    readonly int sequence;

    public string GetId() => ToString();

    public BankTransactionId GetNextId() => FromInt32(sequence + 1);
}
