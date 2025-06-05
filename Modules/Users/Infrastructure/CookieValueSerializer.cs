using Frederikskaj2.Reservations.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NeoSmart.Utils;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace Frederikskaj2.Reservations.Users;

sealed class CookieValueSerializer : IDisposable
{
    readonly Aes aes;
    readonly ILogger logger;
    readonly JsonSerializerOptions serializerOptions;

    public CookieValueSerializer(
        IOptionsSnapshot<CookieOptions> cookieOptions,
        IOptionsSnapshot<JsonSerializerOptions> jsonSerializerOptions,
        ILogger<CookieValueSerializer> logger)
    {
        this.logger = logger;
        serializerOptions = jsonSerializerOptions.Value;

        var key = Convert.FromBase64String(cookieOptions.Value.EncryptionKey ?? "");
        aes = Aes.Create();
        if (8*key.Length != aes.KeySize)
        {
            aes.Dispose();
            throw new ConfigurationException("Invalid encryption key length.");
        }
        aes.Key = key;
        aes.Padding = PaddingMode.ISO10126;
    }

    public string Serialize<T>(T value)
    {
        using var bytes = new MemoryStream();
        bytes.Write(aes.IV, 0, aes.IV.Length);
        using var encryptor = aes.CreateEncryptor();
        using var cryptoStream = new CryptoStream(bytes, encryptor, CryptoStreamMode.Write);
        JsonSerializer.Serialize(cryptoStream, value, serializerOptions);
        cryptoStream.FlushFinalBlock();
        return UrlBase64.Encode(bytes.ToArray());
    }

    public T? Deserialize<T>(string? value) where T : class
    {
        if (value is null)
            return null;
        try
        {
            return UnsafeDeserialize();
        }
        catch (InvalidOperationException exception)
        {
            logger.LogWarning(exception, "Cookie {Cookie} cannot be parsed", value);
            return null;
        }
        catch (CryptographicException exception)
        {
            logger.LogWarning(exception, "Cookie {Cookie} cannot be decrypted", value);
            return null;
        }
        catch (JsonException exception)
        {
            logger.LogWarning(exception, "Cookie {Cookie} cannot be deserialized", value);
            return null;
        }
        catch (FormatException exception)
        {
            logger.LogWarning(exception, "Cookie {Cookie} cannot be deserialized", value);
            return null;
        }

        T? UnsafeDeserialize()
        {
            using var bytes = new MemoryStream(UrlBase64.Decode(value));
            var iv = new byte[aes.BlockSize/8];
            var bytesRead = bytes.Read(iv, 0, iv.Length);
            if (bytesRead < iv.Length)
            {
                logger.LogWarning("Cookie {Cookie} is too short to be decrypted", value);
                return null;
            }
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            using var cryptoStream = new CryptoStream(bytes, decryptor, CryptoStreamMode.Read);
            return JsonSerializer.Deserialize<T>(cryptoStream, serializerOptions);
        }
    }

    public void Dispose() => aes.Dispose();
}
