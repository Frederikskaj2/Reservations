using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Frederikskaj2.Reservations.Server.Domain
{
    internal readonly struct LockBoxCode : IEquatable<LockBoxCode>
    {
        private static readonly ImmutableHashSet<int> allDigits = Enumerable.Range(0, 10).ToImmutableHashSet();

        private readonly ImmutableHashSet<int> code;

        private LockBoxCode(ImmutableHashSet<int> code) => this.code = code;

        public LockBoxCode(IEnumerable<int> digits) => code = digits.ToImmutableHashSet();

        public LockBoxCode(string code) => this.code = code.ToCharArray().Select(c => c - '0').ToImmutableHashSet();

        public int DigitCount => code.Count;

        public bool HasDigit(int digit) => code.Contains(digit);

        public LockBoxCode AddDigit(int digit)
        {
            if (!(0 <= digit && digit <= 9))
                throw new ArgumentOutOfRangeException(nameof(digit), digit, "Digit has to be 0-9");
            return new LockBoxCode(code.Add(digit));
        }

        public LockBoxCode RemoveDigit(int digit)
        {
            if (!(0 <= digit && digit <= 9))
                throw new ArgumentOutOfRangeException(nameof(digit), digit, "Digit has to be 0-9");
            return new LockBoxCode(code.Remove(digit));
        }

        public LockBoxCode Inverse() => new LockBoxCode(allDigits.Except(code));

        public Memory<int> ToMemory() => code.ToArray();

        public bool Equals(LockBoxCode other) => code.SetEquals(other.code);

        public override bool Equals(object? obj) => obj is LockBoxCode code && Equals(code);

        public override int GetHashCode()
            => code.OrderBy(n => n).Aggregate(0, HashCode.Combine);

        public override string ToString()
            => new string(code.OrderBy(digit => digit).Select(digit => (char) ('0' + digit)).ToArray());

        public static LockBoxCodeDifference GetDifference(LockBoxCode lockBoxCode1, LockBoxCode lockBoxCode2)
        {
            var turnOn = lockBoxCode1.code.Except(lockBoxCode2.code);
            var turnOff = lockBoxCode2.code.Except(lockBoxCode1.code);
            return new LockBoxCodeDifference(turnOff, turnOn);
        }
    }
}