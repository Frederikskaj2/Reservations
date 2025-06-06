using System;
using System.Collections.Immutable;
using System.Linq;

namespace Frederikskaj2.Reservations.LockBox;

readonly struct CombinationCodeDifference(ImmutableHashSet<int> turnOff, ImmutableHashSet<int> turnOn)
{
    readonly ImmutableHashSet<int> turnOff = turnOff;
    readonly ImmutableHashSet<int> turnOn = turnOn;

    public override string ToString()
    {
        var length = 3*(turnOff.Count + turnOn.Count) - 1;

        return length > 0 ? string.Create(length, this, SpanAction) : "";

        static void SpanAction(Span<char> span, CombinationCodeDifference state)
        {
            var offset = 0;
            foreach (var digit in state.turnOff.Concat(state.turnOn).OrderBy(d => d > 0 ? d : 10))
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
