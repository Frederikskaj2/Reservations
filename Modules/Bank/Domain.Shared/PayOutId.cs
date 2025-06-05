using Frederikskaj2.Reservations.Core;
using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Bank;

[Primitive(Features.Equatable | Features.Formattable)]
public readonly partial struct PayOutId : IIsId
{
    readonly int id;

    public string GetId() => ToString();
}
