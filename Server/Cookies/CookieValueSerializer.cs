using Frederikskaj2.Reservations.Shared.Core;
using Microsoft.Extensions.Options;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;

namespace Frederikskaj2.Reservations.Server;

public sealed class CookieValueSerializer : IDisposable
{
    readonly Aes aes;
    readonly JsonSerializerOptions serializerOptions;

    public CookieValueSerializer(IOptionsSnapshot<CookieOptions> cookieOptions, IOptionsSnapshot<JsonSerializerOptions> jsonSerializerOptions)
    {
        serializerOptions = jsonSerializerOptions.Value;

        var key = Convert.FromBase64String(cookieOptions.Value.EncryptionKey ?? "");
        aes = Aes.Create();
        if (8*key.Length != aes.KeySize)
            throw new ConfigurationException("Invalid encryption key length.");
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
        return Convert.ToBase64String(bytes.ToArray());
    }

    public T? Deserialize<T>(string? value) where T : class
    {
        if (value is null)
            return null;
        try
        {
            return UnsafeDeserialize();
        }
        catch (FormatException)
        {
            return null;
        }
        catch (CryptographicException)
        {
            return null;
        }
        catch (JsonException)
        {
            return null;
        }

        T? UnsafeDeserialize()
        {
            using var bytes = new MemoryStream(Convert.FromBase64String(value));
            var iv = new byte[aes.BlockSize/8];
            var bytesRead = bytes.Read(iv, 0, iv.Length);
            if (bytesRead < iv.Length)
                return null;
            aes.IV = iv;
            using var decryptor = aes.CreateDecryptor();
            using var cryptoStream = new CryptoStream(bytes, decryptor, CryptoStreamMode.Read);
            return JsonSerializer.Deserialize<T>(cryptoStream, serializerOptions);
        }
    }

    public void Dispose() => aes.Dispose();
}
