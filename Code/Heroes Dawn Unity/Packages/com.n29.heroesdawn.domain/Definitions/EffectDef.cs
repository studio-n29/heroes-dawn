#nullable enable
using System;
using System.Collections.Generic;

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
        /// <summary>Condition d'application de l'effet ; nulle : toujours appliqué.</summary>
        public Condition? When { get; }
        /// <summary>Modificateurs de la valeur de base (liste vide par défaut).</summary>
        public IReadOnlyList<ValueMod> Mods { get; }
        /// <summary>Réaction armée par un effet <see cref="EffectType.ArmReaction"/> ; nulle pour les autres effets. Son effet est mis à l'échelle comme le reste de la carte (<see cref="ScaleReaction"/>).</summary>
        public ReactionDef? Reaction { get; }

        public EffectDef(EffectType type, int value, StatusType status = StatusType.None, int duration = 0, int chancePercent = 100,
            Condition? when = null, IReadOnlyList<ValueMod>? mods = null, ReactionDef? reaction = null)
        {
            Type = type; Value = value; Status = status; Duration = duration; ChancePercent = chancePercent;
            When = when; Mods = mods ?? Array.Empty<ValueMod>(); Reaction = reaction;
        }

        /// <summary>Copie avec valeur, durée et/ou chance remplacées ; tous les autres champs (dont condition, modificateurs et réaction, telle quelle) sont conservés.</summary>
        public EffectDef With(int? value = null, int? duration = null, int? chance = null)
            => new EffectDef(Type, value ?? Value, Status, duration ?? Duration, chance ?? ChancePercent, When, Mods, Reaction);

        /// <summary>
        /// Effet <see cref="EffectType.ArmReaction"/> dont l'effet de la réaction passe par <paramref name="scale"/> (même formule qu'un effet de base de son type) ;
        /// la réaction est reconstruite (tout le reste copié) seulement si l'effet change. Tout autre effet est rendu tel quel.
        /// </summary>
        public EffectDef ScaleReaction(Func<EffectDef, EffectDef> scale)
        {
            if (Type != EffectType.ArmReaction || Reaction == null) return this;
            var e = Reaction.Effect;
            var n = scale(e);
            if (n.Value == e.Value && n.Duration == e.Duration && n.ChancePercent == e.ChancePercent) return this; // inchangé : même réaction (réarmement par référence)
            return new EffectDef(Type, Value, Status, Duration, ChancePercent, When, Mods, Reaction.WithEffect(n));
        }
    }
}
