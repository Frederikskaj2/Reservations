using System;

namespace Frederikskaj2.Reservations.LockBox;

static class MemoryExtensions
{
    public static Memory<T> Shuffle<T>(this Memory<T> memory, Random random) => Shuffle(memory, memory.Length, random);

    public static Memory<T> Shuffle<T>(this Memory<T> memory, int count, Random random)
    {
        if (!(0 < count && count <= memory.Length))
            throw new ArgumentOutOfRangeException(nameof(count));

        var n = Math.Max(count, memory.Length - 2);
        for (var i = 0; i < n; i += 1)
        {
            var j = random.Next(i + 1);
            (memory.Span[j], memory.Span[i]) = (memory.Span[i], memory.Span[j]);
        }

        return memory[..count];
    }
}
