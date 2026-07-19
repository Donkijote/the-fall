using System;
using TheFall.Domain;

namespace TheFall.Infrastructure
{
    /// <summary>
    /// Stable xorshift32 implementation for replayable prototype rule execution.
    /// </summary>
    public sealed class SeededRandomSource : IRandomSource
    {
        private const uint ZeroSeedFallback = 0x6D2B79F5u;
        private uint _state;

        public SeededRandomSource(int seed)
        {
            Seed = seed;
            _state = unchecked((uint)seed);
            if (_state == 0)
            {
                _state = ZeroSeedFallback;
            }
        }

        public int Seed { get; }

        public int NextInt(int exclusiveUpperBound)
        {
            if (exclusiveUpperBound <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(exclusiveUpperBound));
            }

            var bound = (uint)exclusiveUpperBound;
            var rejectionThreshold = unchecked(0u - bound) % bound;
            uint value;

            do
            {
                value = NextUInt32();
            }
            while (value < rejectionThreshold);

            return (int)(value % bound);
        }

        private uint NextUInt32()
        {
            var value = _state;
            value ^= value << 13;
            value ^= value >> 17;
            value ^= value << 5;
            _state = value;
            return value;
        }
    }
}
