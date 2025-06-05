using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.Options;
using NodaTime;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Frederikskaj2.Reservations.Users;

class EncryptedTokenProvider
{
    readonly Encoding encoding = Encoding.UTF8;
    readonly byte[] key;

    public EncryptedTokenProvider(IOptions<TokenEncryptionOptions> options)
    {
        key = Convert.FromBase64String(options.Value.Key);
        if (key.Length is not TokenEncryptionOptions.KeyLength)
            throw new ConfigurationException("Invalid token encryption key length.");
    }

    public ImmutableArray<byte> CreateToken(string purpose, Instant timestamp, byte[] data)
    {
        var hmac = ComputeHmac(purpose, timestamp, data);
        using var stream = new MemoryStream(sizeof(long) + key.Length);
        using (var binaryWriter = new BinaryWriter(stream, encoding))
        {
            binaryWriter.Write(timestamp.ToUnixTimeTicks());
            binaryWriter.Write(hmac);
        }
        return stream.ToArray().UnsafeNoCopyToImmutableArray();
    }

    public TokenValidationResult ValidateToken(string purpose, Instant earliestAcceptableTokenCreationTimestamp, byte[] data, ImmutableArray<byte> token)
    {
        using var stream = new MemoryStream(token.UnsafeNoCopyToArray());
        try
        {
            using var binaryReader = new BinaryReader(stream, encoding);
            var ticks = binaryReader.ReadInt64();
            var providedTimestamp = Instant.FromUnixTimeTicks(ticks);
            var providedHmac = binaryReader.ReadBytes(key.Length);
            if (providedHmac.Length < key.Length)
                return TokenValidationResult.Invalid;
            var hmac = ComputeHmac(purpose, providedTimestamp, data);
            if (!ConstantTimeArrayComparer.AreEqual(providedHmac, hmac))
                return TokenValidationResult.Invalid;
            return providedTimestamp < earliestAcceptableTokenCreationTimestamp ? TokenValidationResult.Expired : TokenValidationResult.Valid;
        }
        catch (EndOfStreamException)
        {
            return TokenValidationResult.Invalid;
        }
    }

    byte[] ComputeHmac(string purpose, Instant timestamp, byte[] data)
    {
        using var stream = new MemoryStream();
        using (var binaryWriter = new BinaryWriter(stream, encoding, leaveOpen: true))
        {
            binaryWriter.Write(purpose);
            binaryWriter.Write(timestamp.ToUnixTimeTicks());
            binaryWriter.Write(data);
        }
        stream.Position = 0L;
        using var hmac = new HMACSHA256(key);
        return hmac.ComputeHash(stream);
    }
}
