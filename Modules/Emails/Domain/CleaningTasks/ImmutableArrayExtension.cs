using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace Frederikskaj2.Reservations.Emails;

static class ImmutableArrayExtension
{
    public static byte[] AsArrayUnsafe(this ImmutableArray<byte> array) =>
        Unsafe.As<ImmutableArray<byte>, byte[]>(ref array);

    public static ImmutableArray<byte> AsImmutableArrayUnsafe(this byte[] array) =>
        Unsafe.As<byte[], ImmutableArray<byte>>(ref array);
}
