using System.Runtime.CompilerServices;

namespace Frederikskaj2.Reservations.Infrastructure;

static class ConstantTimeArrayComparer
{
    //  The method is specifically written so that the loop is not optimized
    //  to protect against timing attacks.
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    public static bool AreEqual(byte[] array1, byte[] array2)
    {
        if (array1.Length != array2.Length)
            return false;
        var areEqual = true;
        for (var i = 0; i < array1.Length; i++)
            areEqual &= array1[i] == array2[i];
        return areEqual;
    }
}