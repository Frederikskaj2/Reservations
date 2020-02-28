using System;
using System.Security.Cryptography;

namespace Frederikskaj2.Reservations.Server.Passwords
{
    internal sealed class CryptographicRandomNumberGenerator : IRandomNumberGenerator, IDisposable
    {
        private readonly RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();

        public byte[] CreateRandomBytes(int count)
        {
            if (count <= 0)
                throw new ArgumentOutOfRangeException(nameof(count));
            var bytes = new byte[count];
            randomNumberGenerator.GetBytes(bytes);
            return bytes;
        }

        public void Dispose() => randomNumberGenerator.Dispose();
    }
}