using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace Frederikskaj2.Reservations.Shared.Web;

public static class JwtTokenParser
{
    const string characterSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789-_";

    static readonly Dictionary<char, byte> lookupTable = characterSet.ToCharArray()
        .Select((c, i) => (c, i)).ToDictionary(tuple => tuple.c, tuple => (byte) tuple.i);

    public static Dictionary<string, JsonElement>? Parse(ReadOnlySpan<char> token)
    {
        var dotIndex = token.IndexOf('.');
        if (dotIndex < 0)
            return null;
        var start = dotIndex + 1;
        var length = token[start..].IndexOf('.');
        if (length < 0)
            return null;
        var span = token[start..(start + length)];
        var bytes = Base64UrlDecode(span);
        var json = Encoding.UTF8.GetString(bytes);
        return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
    }

    static byte[] Base64UrlDecode(ReadOnlySpan<char> span)
    {
        var byteCount = span.Length*6/8;
        var bytes = new byte[byteCount];

        var byteIndex = 0;
        for (var spanIndex = 0; spanIndex < span.Length; spanIndex += 4)
        {
            var sextet0 = lookupTable[span[spanIndex]];
            var sextet1 = lookupTable[span[spanIndex + 1]];
            bytes[byteIndex] = (byte) (sextet0 << 2 | (sextet1 & 0x30) >> 4);
            if (spanIndex == span.Length - 2)
                break;
            var sextet2 = lookupTable[span[spanIndex + 2]];
            bytes[byteIndex + 1] = (byte) ((sextet1 & 0xF) << 4 | (sextet2 & 0x3C) >> 2);
            if (spanIndex == span.Length - 3)
                break;
            var sextet3 = lookupTable[span[spanIndex + 3]];
            bytes[byteIndex + 2] = (byte) ((sextet2 & 0x3) << 6 | sextet3);
            byteIndex += 3;
        }
        return bytes;
    }
}