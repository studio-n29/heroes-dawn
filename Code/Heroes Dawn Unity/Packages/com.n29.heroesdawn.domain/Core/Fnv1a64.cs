namespace N29.HeroesDawn.Core
{
    /// <summary>Empreinte FNV-1a 64 bits, utilisée pour comparer deux états (rejeu).</summary>
    public struct Fnv1a64
    {
        const ulong Offset = 14695981039346656037UL;
        const ulong Prime = 1099511628211UL;
        ulong _hash;
        bool _started;

        public ulong Value => _started ? _hash : Offset;

        public void Add(long v)
        {
            if (!_started) { _hash = Offset; _started = true; }
            for (int i = 0; i < 8; i++)
            {
                _hash ^= (byte)(v >> (i * 8));
                _hash = unchecked(_hash * Prime);
            }
        }

        public void Add(int v) => Add((long)v);

        public void Add(string s)
        {
            foreach (char c in s) Add((long)c);
        }
    }
}
