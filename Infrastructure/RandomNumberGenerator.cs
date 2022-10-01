using Frederikskaj2.Reservations.Application;
using System;

namespace Frederikskaj2.Reservations.Infrastructure;

sealed class RandomNumberGenerator : IRandomNumberGenerator, IDisposable
{
    readonly System.Security.Cryptography.RandomNumberGenerator randomNumberGenerator = System.Security.Cryptography.RandomNumberGenerator.Create();
        
    public byte[] CreateRandomBytes(int count)
    {
        var bytes = new byte[count];
        randomNumberGenerator.GetBytes(bytes);
        return bytes;
    }

    public void Dispose() => randomNumberGenerator.Dispose();
}