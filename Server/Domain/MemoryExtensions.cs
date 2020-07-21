using System;

namespace Frederikskaj2.Reservations.Server.Domain
{
    internal static class MemoryExtensions
    {
        public static Memory<T> Shuffle<T>(this Memory<T> memory, Random random) => Shuffle(memory, memory.Length, random);

        public static Memory<T> Shuffle<T>(this Memory<T> memory, int count, Random random)
        {
            if (random is null)
                throw new ArgumentNullException(nameof(random));
            if (!(0 < count && count <= memory.Length))
                throw new ArgumentOutOfRangeException(nameof(count));

            var n = Math.Max(count, memory.Length - 2);
            for (var i = 0; i < n; i += 1)
            {
                var j = random.Next(i + 1);
                var value = memory.Span[j];
                memory.Span[j] = memory.Span[i];
                memory.Span[i] = value;
            }
            return memory.Slice(0, count);
        }
    }
}