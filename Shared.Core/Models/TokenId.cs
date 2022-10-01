using Liversage.Primitives;

namespace Frederikskaj2.Reservations.Shared.Core;

[Primitive]
public readonly partial struct TokenId
{
    readonly int id;

    public TokenId NextId => id + 1;
}
