#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Core;

namespace N29.HeroesDawn.Definitions
{
    public sealed class SkillDef
    {
        public SkillId Id { get; }
        public SkillSlot Slot { get; }
        /// <summary>Recharge historique en tours (ne bloque plus la compétence : seule l énergie la limite ; sert à dériver le coût par défaut).</summary>
        public int Cooldown { get; }
        /// <summary>Énergie dépensée à chaque usage. Sans coût explicite, dérivé de la recharge historique (10 par tour de recharge).</summary>
        public int Cost { get; }
        public TargetRule Target { get; }
        /// <summary>Capacités de la compétence, dans l'ordre. Le constructeur historique en produit une seule (OnCast).</summary>
        public IReadOnlyList<Ability> Abilities { get; }
        /// <summary>Le premier effet est l'effet de base ; les suivants sont les sous-effets (altérations, faiblesses…). Vue : concaténation ordonnée des effets des capacités OnCast, calculée une fois.</summary>
        public IReadOnlyList<EffectDef> Effects { get; }

        public SkillDef(SkillId id, SkillSlot slot, int cooldown, TargetRule target, IReadOnlyList<EffectDef> effects, int cost = -1)
            : this(id, slot, cooldown, target, new[] { new Ability(AbilityTrigger.OnCast, effects) }, cost, true) { }

        SkillDef(SkillId id, SkillSlot slot, int cooldown, TargetRule target, IReadOnlyList<Ability> abilities, int cost, bool _)
        {
            Id = id; Slot = slot; Cooldown = cooldown; Target = target; Abilities = abilities;
            Cost = cost >= 0 ? cost : cooldown * 10;
            Effects = abilities.Count == 1 && abilities[0].Trigger == AbilityTrigger.OnCast ? abilities[0].Effects : Concat(abilities, AbilityTrigger.OnCast);
        }

        public static SkillDef FromAbilities(SkillId id, SkillSlot slot, int cooldown, TargetRule target, IReadOnlyList<Ability> abilities, int cost = -1)
            => new SkillDef(id, slot, cooldown, target, abilities, cost, true);

        /// <summary>Effets, dans l'ordre, des capacités du déclencheur donné.</summary>
        public IReadOnlyList<EffectDef> EffectsOf(AbilityTrigger trigger)
            => trigger == AbilityTrigger.OnCast ? Effects : Concat(Abilities, trigger);

        /// <summary>Même structure (capacités, ordre, id, slot, recharge, cible, coût) avec chaque effet transformé.</summary>
        public SkillDef MapEffects(Func<EffectDef, EffectDef> f)
        {
            var mapped = new Ability[Abilities.Count];
            for (int i = 0; i < mapped.Length; i++)
            {
                var src = Abilities[i].Effects;
                var fx = new EffectDef[src.Count];
                for (int j = 0; j < fx.Length; j++) fx[j] = f(src[j]);
                mapped[i] = new Ability(Abilities[i].Trigger, fx);
            }
            return new SkillDef(Id, Slot, Cooldown, Target, mapped, Cost, true);
        }

        /// <summary>Copie avec des capacités ajoutées après les existantes.</summary>
        public SkillDef WithAbilities(IEnumerable<Ability> extra)
        {
            var all = new List<Ability>(Abilities);
            all.AddRange(extra);
            return new SkillDef(Id, Slot, Cooldown, Target, all, Cost, true);
        }

        static IReadOnlyList<EffectDef> Concat(IReadOnlyList<Ability> abilities, AbilityTrigger trigger)
        {
            var list = new List<EffectDef>();
            foreach (var a in abilities) if (a.Trigger == trigger) list.AddRange(a.Effects);
            return list;
        }

        public EffectDef? BaseEffect => Effects.Count > 0 ? Effects[0] : null;

        /// <summary>Effets secondaires après l'effet de base.</summary>
        public IEnumerable<EffectDef> SubEffects
        {
            get { for (int i = 1; i < Effects.Count; i++) yield return Effects[i]; }
        }

        public SkillCategory Category => SkillCategories.Of(Target, BaseEffect);
    }

    /// <summary>Catégorie déduite de l'effet de base : pas de donnée supplémentaire à maintenir.</summary>
    public static class SkillCategories
    {
        public static SkillCategory Of(TargetRule target, EffectDef? baseEffect)
        {
            if (baseEffect == null) return SkillCategory.Damage;
            switch (baseEffect.Type)
            {
                case EffectType.Heal: return SkillCategory.Heal;
                case EffectType.Shield: return SkillCategory.Protection;
                case EffectType.GaugeDelta: return SkillCategory.TimeManagement;
                case EffectType.ApplyStatus:
                    if (target == TargetRule.SingleEnemy || target == TargetRule.AllEnemies) return SkillCategory.Damage;
                    return IsDefensive(baseEffect.Status) ? SkillCategory.Protection : SkillCategory.Boost;
                default: return SkillCategory.Damage;
            }
        }

        static bool IsDefensive(StatusType s) => s == StatusType.Taunt || s == StatusType.Evade || s == StatusType.Anchor || s == StatusType.Shield;
    }
}
