using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Shared.Core;

[Primitive(Features.Equatable | Features.Formattable)]
public readonly partial struct UserId
{
    readonly int id;
}
