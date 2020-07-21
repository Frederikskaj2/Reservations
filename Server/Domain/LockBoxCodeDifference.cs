using System;
using System.Collections.Immutable;
using System.Linq;

namespace Frederikskaj2.Reservations.Server.Domain
{
    internal readonly struct LockBoxCodeDifference
    {
        private readonly ImmutableHashSet<int> turnOff;
        private readonly ImmutableHashSet<int> turnOn;

        public LockBoxCodeDifference(ImmutableHashSet<int> turnOff, ImmutableHashSet<int> turnOn)
        {
            this.turnOff = turnOff;
            this.turnOn = turnOn;
        }

        public override string ToString()
        {
            var length = 3*(turnOff.Count + turnOn.Count) - 1;

            return length > 0 ? string.Create(length, this, SpanAction) : string.Empty;

            static void SpanAction(Span<char> span, LockBoxCodeDifference state)
            {
                var offset = 0;
                foreach (var digit in state.turnOff.Concat(state.turnOn).OrderBy(d => d))
                {
                    const char plus = '+';
                    const char minus = '\u2212';
                    span[offset++] = state.turnOn.Contains(digit) ? plus : minus;
                    span[offset++] = (char) ('0' + digit);
                    if (offset < span.Length)
                        span[offset++] = ' ';
                }
            }
        }
    }
}