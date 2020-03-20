using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Frederikskaj2.Reservations.Shared;
using Microsoft.Extensions.Options;
using NodaTime;

namespace Frederikskaj2.Reservations.Server.Passwords
{
    internal class EncryptedTokenProvider : IEncryptedTokenProvider
    {
        private readonly Encoding encoding = Encoding.UTF8;
        private readonly byte[] key;

        public EncryptedTokenProvider(IOptions<PasswordOptions> options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            if (options.Value.TokenEncryptionKey == null)
                throw new Server.ConfigurationException("Missing token encryption key.");
            key = Convert.FromBase64String(options.Value.TokenEncryptionKey);
            if (key.Length != PasswordOptions.TokenEncryptionKeyLength)
                throw new Server.ConfigurationException("Invalid token encryption key length.");
        }

        public string CreateToken(string purpose, Instant timestamp, byte[] data)
        {
            var hmac = ComputeHmac(purpose, timestamp, data);
            using var stream = new MemoryStream(sizeof(long) + PasswordOptions.TokenEncryptionKeyLength);
            using (var binaryWriter = new BinaryWriter(stream, encoding))
            {
                binaryWriter.Write(timestamp.ToUnixTimeTicks());
                binaryWriter.Write(hmac);
            }
            return Convert.ToBase64String(stream.ToArray());
        }

        public TokenValidationResult ValidateToken(
            string purpose, Instant earliestAcceptableTokenCreationTimestamp, byte[] data, string token)
        {
            using var stream = new MemoryStream(Convert.FromBase64String(token));
            try
            {
                using var binaryReader = new BinaryReader(stream, encoding);
                var ticks = binaryReader.ReadInt64();
                var providedHmac = binaryReader.ReadBytes(PasswordOptions.TokenEncryptionKeyLength);
                if (providedHmac.Length < PasswordOptions.TokenEncryptionKeyLength)
                    return TokenValidationResult.Failure;
                var providedTimestamp = Instant.FromUnixTimeTicks(ticks);
                var hmac = ComputeHmac(purpose, providedTimestamp, data);
                if (!CryptographicOperations.FixedTimeEquals(providedHmac, hmac))
                    return TokenValidationResult.Failure;
                return providedTimestamp < earliestAcceptableTokenCreationTimestamp ? TokenValidationResult.TooOld : TokenValidationResult.Success;
            }
            catch (EndOfStreamException)
            {
                return TokenValidationResult.Failure;
            }
        }

        private byte[] ComputeHmac(string purpose, Instant timestamp, byte[] data)
        {
            using var stream = new MemoryStream();
            using (var binaryWriter = new BinaryWriter(stream, encoding, true))
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
}