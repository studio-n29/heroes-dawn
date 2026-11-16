#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Moment où une capacité se déclenche (P0 : seulement à l'usage de la compétence).</summary>
    public enum AbilityTrigger { OnCast }

    /// <summary>Capacité d'une compétence : un déclencheur et les effets qu'il produit, dans l'ordre.</summary>
    public sealed class Ability
    {
        public AbilityTrigger Trigger { get; }
        public IReadOnlyList<EffectDef> Effects { get; }

        public Ability(AbilityTrigger trigger, IReadOnlyList<EffectDef> effects)
        {
            Trigger = trigger; Effects = effects;
        }
    }
}
