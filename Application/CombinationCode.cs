using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Frederikskaj2.Reservations.Application;

readonly struct CombinationCode : IEquatable<CombinationCode>
{
    static readonly ImmutableHashSet<int> allDigits = Enumerable.Range(0, 10).ToImmutableHashSet();

    readonly ImmutableHashSet<int> code;

    CombinationCode(ImmutableHashSet<int> code) => this.code = code;

    public CombinationCode(IEnumerable<int> digits) => code = digits.ToImmutableHashSet();

    public CombinationCode(string code) => this.code = code.ToCharArray().Select(c => c - '0').ToImmutableHashSet();

    public int DigitCount => code.Count;

    public bool HasDigit(int digit) => code.Contains(digit);

    public CombinationCode AddDigit(int digit)
    {
        if (digit is not (>= 0 and <= 9))
            throw new ArgumentOutOfRangeException(nameof(digit), digit, "Digit has to be 0-9.");
        return new CombinationCode(code.Add(digit));
    }

    public CombinationCode RemoveDigit(int digit)
    {
        if (digit is not (>= 0 and <= 9))
            throw new ArgumentOutOfRangeException(nameof(digit), digit, "Digit has to be 0-9.");
        return new CombinationCode(code.Remove(digit));
    }

    public CombinationCode Inverse() => new(allDigits.Except(code));

    public Memory<int> ToMemory() => code.ToArray();

    public bool Equals(CombinationCode other) => code.SetEquals(other.code);

    public override bool Equals(object? obj) => obj is CombinationCode code && Equals(code);

    public override int GetHashCode()
        => code.OrderBy(n => n).Aggregate(0, HashCode.Combine);

    public override string ToString()
        // Digit order: 1234567890. Matches the order on the lock box.
        => new(code.OrderBy(digit => (digit + 9)%10).Select(digit => (char) ('0' + digit)).ToArray());

    public static CombinationCodeDifference GetDifference(CombinationCode combinationCode1, CombinationCode combinationCode2)
    {
        var turnOn = combinationCode1.code.Except(combinationCode2.code);
        var turnOff = combinationCode2.code.Except(combinationCode1.code);
        return new CombinationCodeDifference(turnOff, turnOn);
    }
}
