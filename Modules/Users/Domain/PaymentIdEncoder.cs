using System;
using System.Collections.Generic;
using System.Linq;

namespace Frederikskaj2.Reservations.Users;

public static class PaymentIdEncoder
{
    // The payment ID is the user ID converted to a short code consisting of
    // upper case letters and numbers. To make the ID recognizable, it's prefixed
    // by "B-", e.g. "B-TSA9".
    //
    // User IDs are obfuscated by using the state transformation function of
    // a linear-feedback shift register that has a cycle of 0x8000. This
    // limits the valid user IDs to the range 0 <= user ID <= 32767.
    //
    // The transformation uses a "xor shift" as described in
    // https://www.jstatsoft.org/v11/i05/paper.
    //
    // This particular transformation has the following form:
    //
    // T = (I + R^3)(I + L^9)(I + R^5)
    //
    // Right xor shift by 3, left xor shift by 9, right xor shift by 5.
    //
    // This transformation can be inverted by inverting T. I don't think
    // this can be simplified to a few lines of code like the forward
    // transformation, and at least here the full matrix multiplication is
    // performed.
    //
    // The resulting 15-bit value is converted to a three-character
    // alphanumeric string using a five-bit character set where potentially
    // ambiguous characters have been removed.
    //
    // In addition, a 5-bit Pearson hash is computed from the three characters
    // and appended to create a four-character identifier. This hash is used to
    // validate a provided identifier.

    const string characterSet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    const char prefix = 'B';
    const char prefixSeparator = '-';

    static readonly Dictionary<char, int> characterToFiveBitValueMap = characterSet.Select(KeyValuePair.Create).ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    static readonly HashSet<char> validCharacters = [..characterSet.ToCharArray()];

    // Masks used to perform the matrix multiplication required for the
    // reverse transform.
    static readonly int[] rowMasks =
    [
        0x4420, 0x6210, 0x3108, 0x5CA4,
        0x6E52, 0x7729, 0x7DA4, 0x7ED2,
        0x7F69, 0x7984, 0x3CC2, 0x5E61,
        0x6900, 0x3480, 0x5A40,
    ];

    // 5-bit Pearson hash table.
    static readonly byte[] pearsonHashTable =
    [
        0x07, 0x1D, 0x1B, 0x13, 0x0A, 0x1C, 0x03, 0x1A,
        0x10, 0x16, 0x06, 0x18, 0x02, 0x09, 0x14, 0x11,
        0x17, 0x0F, 0x0E, 0x04, 0x19, 0x08, 0x00, 0x12,
        0x0C, 0x15, 0x05, 0x01, 0x0B, 0x0D, 0x1E, 0x1F,
    ];

    public static PaymentId FromUserId(UserId userId)
    {
        if (userId.ToInt32() is not (>= 0 and <= 0x7FFF))
            throw new ArgumentOutOfRangeException(nameof(userId), userId, "User ID is not in the range 0 to 32,767 (both inclusive).");

        var id = (short) userId.ToInt32();
        var obfuscatedId = XorShift(id);
        var hash = GetPearsonHash(obfuscatedId);
        var obfuscatedIdWithHash = hash << 15 | (ushort) obfuscatedId;
        return PaymentId.FromString(Encode(obfuscatedIdWithHash));

        static short XorShift(short value)
        {
            value ^= (short) (value >> 3 & 0x7FFF);
            value ^= (short) (value << 9 & 0x7FFF);
            value ^= (short) (value >> 5 & 0x7FFF);
            return value;
        }

        static string Encode(int value)
        {
            return string.Create(6, value, SpanAction);

            static void SpanAction(Span<char> span, int state)
            {
                span[0] = prefix;
                span[1] = prefixSeparator;
                for (var i = 2; i < 6; i += 1)
                {
                    var fiveBitValue = state & 0x1F;
                    state >>= 5;
                    span[i] = characterSet[fiveBitValue];
                }
            }
        }
    }

    public static UserId ToUserId(PaymentId paymentId)
    {
        var (obfuscatedId, isValid) = Validate(paymentId);
        if (!isValid)
            throw new ArgumentException($"Invalid payment ID {paymentId}.", nameof(paymentId));
        return UserId.FromInt32(InverseXorShift(obfuscatedId));

        static short InverseXorShift(short value)
        {
            short result = 0;
            for (var row = 0; row < rowMasks.Length; row += 1)
            {
                result <<= 1;
                var maskedValue = value & rowMasks[row];
                var oddNumberOfBits = CountBits((short) maskedValue)%2 is 1;
                if (oddNumberOfBits)
                    result |= 1;
            }
            return result;
        }

        // https://graphics.stanford.edu/~seander/bithacks.html#CountBitsSet64 option 2.
        static ulong CountBits(short v) =>
            ((ulong) (v & 0xFFF)*0x1001001001001ul & 0x84210842108421ul)%0x1F +
            (((ulong) (v & 0xFFF000) >> 12)*0x1001001001001ul & 0x84210842108421ul)%0x1F;
    }

    public static bool IsValid(PaymentId paymentId) =>
        Validate(paymentId).IsValid;

    static (short ObfuscatedId, bool IsValid) Validate(PaymentId paymentId)
    {
        var id = paymentId.ToString()!;
        var isValid = id.Length is 6 &&
                      id[0] is prefix &&
                      id[1] is prefixSeparator &&
                      validCharacters.Contains(id[2]) &&
                      validCharacters.Contains(id[3]) &&
                      validCharacters.Contains(id[4]) &&
                      validCharacters.Contains(id[5]);
        if (!isValid)
            return (0, false);

        var obfuscatedIdWithHash = Decode(id.AsSpan()[2..]);
        var obfuscatedId = (short) (obfuscatedIdWithHash & 0x7FFF);
        var expectedHash = GetPearsonHash(obfuscatedId);
        var hash = obfuscatedIdWithHash >> 15;
        return (obfuscatedId, hash == expectedHash);

        static int Decode(ReadOnlySpan<char> code)
        {
            var value = 0;
            for (var i = 0; i < 4; i += 1)
            {
                var fiveBitValue = characterToFiveBitValueMap[code[i]];
                value |= fiveBitValue << i*5;
            }
            return value;
        }
    }

    static byte GetPearsonHash(short value) =>
        pearsonHashTable[pearsonHashTable[pearsonHashTable[value & 0x1F] ^ value >> 5 & 0x1F] ^ value >> 10 & 0x1F];
}
