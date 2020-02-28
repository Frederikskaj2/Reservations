using System;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;

namespace Frederikskaj2.Reservations.Server.Passwords
{
    internal class PasswordHasher : IPasswordHasher
    {
        // Version 3:
        // PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 20000 iterations (configurable).
        // Format: { 0x01, prf (UInt32), iteration count (UInt32), salt length (UInt32), salt, subkey }
        // (All UInt32s are stored big-endian.)

        private const int FormatMarker = 0x01;
        private readonly PasswordOptions options;
        private readonly IRandomNumberGenerator randomNumberGenerator;

        public PasswordHasher(IRandomNumberGenerator randomNumberGenerator, IOptions<PasswordOptions> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            this.randomNumberGenerator =
                randomNumberGenerator ?? throw new ArgumentNullException(nameof(randomNumberGenerator));

            this.options = options.Value;

            if (this.options.PasswordIterationCount < 1)
                throw new ConfigurationException("Invalid password hash iteration count.");
        }

        public string HashPassword(string password)
        {
            if (password is null)
                throw new ArgumentNullException(nameof(password));

            return Convert.ToBase64String(HashPasswordV3(password));
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            if (hashedPassword is null)
                throw new ArgumentNullException(nameof(hashedPassword));
            if (providedPassword is null)
                throw new ArgumentNullException(nameof(providedPassword));

            var decodedHashedPassword = Convert.FromBase64String(hashedPassword);

            // Read the format marker from the hashed password.
            if (decodedHashedPassword.Length == 0)
                return PasswordVerificationResult.Failed;
            switch (decodedHashedPassword[0])
            {
                case FormatMarker:
                    if (VerifyHashedPasswordV3(decodedHashedPassword, providedPassword, out var iterationCount))
                        return iterationCount < options.PasswordIterationCount
                            ? PasswordVerificationResult.SuccessRehashNeeded
                            : PasswordVerificationResult.Success;
                    else
                        return PasswordVerificationResult.Failed;

                default:
                    return PasswordVerificationResult.Failed; // Unknown format marker.
            }
        }

        private byte[] HashPasswordV3(string password) =>
            HashPasswordV3(
                password, randomNumberGenerator, KeyDerivationPrf.HMACSHA256, options.PasswordIterationCount, 128/8,
                256/8);

        private static byte[] HashPasswordV3(
            string password, IRandomNumberGenerator randomNumberGenerator, KeyDerivationPrf prf, int iterationCount,
            int saltSize, int numberBytesRequested)
        {
            // Produce a version 3 (see comment above) text hash.
            var salt = randomNumberGenerator.CreateRandomBytes(saltSize);
            var subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterationCount, numberBytesRequested);

            var outputBytes = new byte[13 + salt.Length + subkey.Length];
            outputBytes[0] = FormatMarker;
            WriteNetworkByteOrder(outputBytes, 1, (uint) prf);
            WriteNetworkByteOrder(outputBytes, 5, (uint) iterationCount);
            WriteNetworkByteOrder(outputBytes, 9, (uint) saltSize);
            Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
            Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
            return outputBytes;
        }

        [SuppressMessage(
            "Design", "CA1031:Do not catch general exception types",
            Justification = "This method should not pass exceptions to callers.")]
        private static bool VerifyHashedPasswordV3(byte[] hashedPassword, string password, out int iterationCount)
        {
            iterationCount = default;

            try
            {
                // Read header information.
                var prf = (KeyDerivationPrf) ReadNetworkByteOrder(hashedPassword, 1);
                iterationCount = (int) ReadNetworkByteOrder(hashedPassword, 5);
                var saltLength = (int) ReadNetworkByteOrder(hashedPassword, 9);

                // Read the salt: must be >= 128 bits.
                if (saltLength < 128/8)
                    return false;
                var salt = new byte[saltLength];
                Buffer.BlockCopy(hashedPassword, 13, salt, 0, salt.Length);

                // Read the subkey (the rest of the payload): must be >= 128 bits.
                var subkeyLength = hashedPassword.Length - 13 - salt.Length;
                if (subkeyLength < 128/8)
                    return false;
                var expectedSubkey = new byte[subkeyLength];
                Buffer.BlockCopy(hashedPassword, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

                // Hash the incoming password and verify it.
                var actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterationCount, subkeyLength);
                return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
            }
            catch
            {
                // This should never occur except in the case of a malformed
                // payload, where we might go off the end of the array.
                // Regardless, a malformed payload implies verification failed.
                return false;
            }
        }

        private static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte) (value >> 24);
            buffer[offset + 1] = (byte) (value >> 16);
            buffer[offset + 2] = (byte) (value >> 8);
            buffer[offset + 3] = (byte) value;
        }

        private static uint ReadNetworkByteOrder(byte[] buffer, int offset) =>
            (uint) buffer[offset + 0] << 24
            | (uint) buffer[offset + 1] << 16
            | (uint) buffer[offset + 2] << 8
            | buffer[offset + 3];
    }
}