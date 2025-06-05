// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
//
// https://github.com/dotnet/aspnetcore/blob/master/src/Identity/Extensions.Core/src/PasswordHasher.cs

using LanguageExt;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.Extensions.Options;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Frederikskaj2.Reservations.Users;

class PasswordHasher(IOptions<PasswordOptions> options, RandomNumberGenerator randomNumberGenerator) : IPasswordHasher
{
    // Version 3:
    // PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 20000 iterations (configurable).
    // Format: { 0x01, prf (UInt32), iteration count (UInt32), salt length (UInt32), salt, subkey }
    // (All UInt32s are stored big-endian.)

    const int formatMarker = 0x01;
    readonly int defaultIterationCount = options.Value.Hashing.IterationCount;

    public Seq<byte> HashPassword(string password) =>
        HashPasswordV3(password, KeyDerivationPrf.HMACSHA256, defaultIterationCount, 128/8, 256/8)
            .ToSeq();

    public PasswordVerificationResult VerifyHashedPassword(Seq<byte> hashedPassword, string providedPassword)
    {
        // Read the format marker from the hashed password.
        if (hashedPassword.Length is 0)
            return PasswordVerificationResult.Failed;
        switch (hashedPassword[0])
        {
            case formatMarker:
                if (VerifyHashedPasswordV3(hashedPassword.ToArray(), providedPassword, out var iterationCount))
                    return iterationCount < defaultIterationCount
                        ? PasswordVerificationResult.SuccessRehashNeeded
                        : PasswordVerificationResult.Success;
                return PasswordVerificationResult.Failed;

            default:
                return PasswordVerificationResult.Failed; // Unknown format marker.
        }
    }

    byte[] HashPasswordV3(string password, KeyDerivationPrf prf, int iterationCount, int saltSize, int numberBytesRequested)
    {
        var salt = randomNumberGenerator.CreateRandomBytes(saltSize);
        var subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterationCount, numberBytesRequested);

        var outputBytes = new byte[13 + salt.Length + subkey.Length];
        outputBytes[0] = formatMarker;
        WriteNetworkByteOrder(outputBytes, 1, (uint) prf);
        WriteNetworkByteOrder(outputBytes, 5, (uint) iterationCount);
        WriteNetworkByteOrder(outputBytes, 9, (uint) saltSize);
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);
        return outputBytes;
    }

    [SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "This method should not pass exceptions to callers.")]
    static bool VerifyHashedPasswordV3(byte[] hashedPassword, string password, out int iterationCount)
    {
        iterationCount = 0;
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
            return ConstantTimeArrayComparer.AreEqual(actualSubkey, expectedSubkey);
        }
        catch
        {
            // This should never occur except in the case of a malformed
            // payload, where we might go off the end of the array.
            // Regardless, a malformed payload implies verification failed.
            return false;
        }
    }

    static void WriteNetworkByteOrder(byte[] buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte) (value >> 24);
        buffer[offset + 1] = (byte) (value >> 16);
        buffer[offset + 2] = (byte) (value >> 8);
        buffer[offset + 3] = (byte) (value >> 0);
    }

    static uint ReadNetworkByteOrder(byte[] buffer, int offset)
        => (uint) buffer[offset + 0] << 24
           | (uint) buffer[offset + 1] << 16
           | (uint) buffer[offset + 2] << 8
           | buffer[offset + 3];
}
