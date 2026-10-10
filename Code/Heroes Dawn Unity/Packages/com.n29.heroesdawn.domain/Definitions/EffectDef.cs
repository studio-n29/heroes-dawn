namespace N29.HeroesDawn.Definitions
{
    public sealed class EffectDef
    {
        public EffectType Type { get; }
        /// <summary>Damage : % ATQ · Heal/Shield : % PV max de la cible · GaugeDelta : points sur 1 000 · PushBack/Advance : % de la frise.</summary>
        public int Value { get; }
        public StatusType Status { get; }
        public int Duration { get; }
        public int ChancePercent { get; }

        public EffectDef(EffectType type, int value, StatusType status = StatusType.None, int duration = 0, int chancePercent = 100)
        {
            Type = type; Value = value; Status = status; Duration = duration; ChancePercent = chancePercent;
        }
    }
}
