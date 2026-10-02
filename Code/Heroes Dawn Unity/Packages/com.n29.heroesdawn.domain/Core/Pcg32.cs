using System;

namespace N29.HeroesDawn.Core
{
    /// <summary>PCG-XSH-RR 64/32 (M. O'Neill). Range sans biais.</summary>
    public sealed class Pcg32 : IRandom
    {
        ulong _state;
        readonly ulong _inc;

        public Pcg32(ulong seed, ulong stream = 54)
        {
            _state = 0UL;
            _inc = (stream << 1) | 1UL;
            NextUInt();
            _state = unchecked(_state + seed);
            NextUInt();
        }

        public ulong State => _state;

        public uint NextUInt()
        {
            ulong old = _state;
            _state = unchecked(old * 6364136223846793005UL + _inc);
            uint xorshifted = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return (xorshifted >> rot) | (xorshifted << ((-rot) & 31));
        }

        public int Range(int minInclusive, int maxExclusive)
        {
            if (maxExclusive <= minInclusive) throw new ArgumentOutOfRangeException(nameof(maxExclusive));
            uint bound = (uint)(maxExclusive - minInclusive);
            uint threshold = unchecked(0u - bound) % bound;
            while (true)
            {
                uint r = NextUInt();
                if (r >= threshold) return minInclusive + (int)(r % bound);
            }
        }

        public bool Chance(int percent)
        {
            if (percent <= 0) return false;
            if (percent >= 100) return true;
            return Range(0, 100) < percent;
        }

        public IRandom Fork(ulong stream)
        {
            ulong seed = ((ulong)NextUInt() << 32) | NextUInt();
            return new Pcg32(seed, stream);
        }
    }
}
