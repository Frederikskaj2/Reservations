using Liversage.Primitives;
using System;

namespace Frederikskaj2.Reservations.Core;

[Primitive]
public readonly partial struct ETag
{
    readonly string eTag;

    public ETag(string eTag)
    {
        if (!IsValid(eTag))
            throw new ArgumentException($"'{eTag}' is not a valid ETag.", nameof(eTag));
        this.eTag = eTag;
    }

    public static bool IsValid(string eTag) =>
        eTag is { Length: > 2 } && eTag[0] is '"' && eTag[^1] is '"';
}
