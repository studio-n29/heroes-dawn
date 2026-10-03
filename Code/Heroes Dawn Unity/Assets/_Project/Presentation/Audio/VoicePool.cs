#nullable enable
namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Choix d'une voix d'effet (C# pur, tableaux fixes, aucune allocation). Le nombre de voix est borné pour le mobile :
    /// une voix libre d'abord, sinon on vole la plus ancienne de priorité inférieure ou égale, sinon l'effet est abandonné.
    /// Un même effet ne dépasse pas sa limite d'instances et respecte son délai (plusieurs coups dans le même tick = un son).
    /// </summary>
    public sealed class VoicePool
    {
        readonly SfxId[] _sfx;
        readonly int[] _priority;
        readonly float[] _startedAt;
        readonly float[] _lastPlayed;

        public VoicePool(int voices, int sfxSlots)
        {
            _sfx = new SfxId[voices];
            _priority = new int[voices];
            _startedAt = new float[voices];
            _lastPlayed = new float[sfxSlots];
            for (int i = 0; i < _lastPlayed.Length; i++) _lastPlayed[i] = float.NegativeInfinity;
        }

        public int Count => _sfx.Length;

        /// <summary>
        /// Voix à utiliser pour <paramref name="sfx"/>, ou -1. <paramref name="playing"/> : état des voix (rempli par l'appelant).
        /// Plus <paramref name="priority"/> est grand, plus l'effet est important.
        /// </summary>
        public int Acquire(SfxId sfx, int priority, int maxInstances, float cooldown, float now, bool[] playing)
        {
            int slot = (int)sfx;
            if (slot >= 0 && slot < _lastPlayed.Length && now - _lastPlayed[slot] < cooldown) return -1;

            int instances = 0, free = -1, steal = -1;
            for (int i = 0; i < _sfx.Length; i++)
            {
                if (!playing[i])
                {
                    if (free < 0) free = i;
                    continue;
                }
                if (_sfx[i] == sfx) instances++;
                if (_priority[i] <= priority && (steal < 0 || _startedAt[i] < _startedAt[steal])) steal = i;
            }
            if (instances >= maxInstances)
            {
                // Limite atteinte : on relance la plus ancienne instance du même effet plutôt que d'en empiler une autre.
                steal = -1;
                for (int i = 0; i < _sfx.Length; i++)
                    if (playing[i] && _sfx[i] == sfx && (steal < 0 || _startedAt[i] < _startedAt[steal])) steal = i;
                free = -1;
            }
            int voice = free >= 0 ? free : steal;
            if (voice < 0) return -1;
            _sfx[voice] = sfx;
            _priority[voice] = priority;
            _startedAt[voice] = now;
            if (slot >= 0 && slot < _lastPlayed.Length) _lastPlayed[slot] = now;
            return voice;
        }
    }
}
