#nullable enable
using System.Collections.Generic;
using System.Text;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Statistiques de l'agent qui porte la compétence : permettent d'afficher les montants bruts (Atk × valeur, PV max × valeur).</summary>
    public readonly struct SkillOwnerStats
    {
        public readonly int Atk;
        public readonly int MaxHp;
        public SkillOwnerStats(int atk, int maxHp) { Atk = atk; MaxHp = maxHp; }
    }

    /// <summary>Description courte d'une compétence, générée à partir de ses effets (évite 40 textes à maintenir).</summary>
    public static class SkillText
    {
        /// <summary>Seuil de PV de l'exécution affiché dans les textes (valeur d'équilibrage par défaut).</summary>
        static readonly int ExecuteThreshold = new BalanceConfig().ExecuteThresholdPercent;

        public static string Describe(SkillDef s, SkillOwnerStats? owner = null)
        {
            var sb = new StringBuilder();
            sb.Append(Loc.T("target." + s.Target));
            if (s.Pick != TargetPick.Default)
            {
                sb.Append(" · ");
                sb.Append(Loc.F("pick." + s.Pick, s.PickStatus == StatusType.None ? "" : Loc.StatusName(s.PickStatus)));
            }
            foreach (var line in Effects(s.Effects, owner, s.Target))
            {
                sb.Append(" · ");
                sb.Append(line);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Avec les statistiques de l'agent, les dégâts, soins et boucliers sont des montants bruts ; sans elles, des pourcentages. Un effet conditionnel est préfixé de sa condition ; ses modificateurs suivent, une ligne chacun.
        /// <paramref name="cardTarget"/> : cible de la carte, pour préciser qu'une réaction d'une carte ZA ou 1A n'est armée que sur le lanceur.
        /// </summary>
        public static IEnumerable<string> Effects(IReadOnlyList<EffectDef> effects, SkillOwnerStats? owner = null, TargetRule? cardTarget = null)
        {
            foreach (var fx in effects)
            {
                string? line = fx.Type == EffectType.ArmReaction ? (fx.Reaction == null ? null : ReactionLine(fx.Reaction, cardTarget)) : BaseLine(fx, owner);
                if (line == null) continue;
                yield return fx.When != null ? Loc.F("fx.conditional", ConditionText(fx.When), line) : line;
                foreach (var mod in fx.Mods) yield return ModLine(fx, mod, owner);
            }
        }

        static string ConditionText(Condition c)
            => Loc.F("cond." + c.Kind, c.Status == StatusType.None ? "" : Loc.StatusName(c.Status), c.Value);

        /// <summary>Bonus d'un modificateur dans l'unité de la ligne de base : mêmes conversions (brut avec les stats de l'agent, jauge en dixièmes de point).</summary>
        static string BonusText(EffectDef fx, int bonus, SkillOwnerStats? owner)
        {
            switch (fx.Type)
            {
                case EffectType.Damage when owner.HasValue: return Loc.F("fx.bonus_damage_raw", owner.Value.Atk * bonus / 100);
                case EffectType.Heal when owner.HasValue: return Loc.F("fx.bonus_heal_raw", owner.Value.MaxHp * bonus / 100);
                case EffectType.Shield when owner.HasValue: return Loc.F("fx.bonus_shield_raw", owner.Value.MaxHp * bonus / 100);
                case EffectType.GaugeDelta: return Loc.F("fx.bonus_pct", fx.Value < 0 ? -(bonus / 10) : bonus / 10); // ralentissement : le bonus l'AUGMENTE, le signe de la base est repris
                default: return Loc.F("fx.bonus_pct", bonus);
            }
        }

        /// <summary>Grandeur comptée (l'échelle d'archétype a sa propre phrase, voir <see cref="ModLine"/>).</summary>
        static string ScaleText(ValueMod m) => Loc.T("scale." + m.Per!.Value);

        static string ModLine(EffectDef fx, ValueMod m, SkillOwnerStats? owner)
        {
            string bonus = BonusText(fx, m.Bonus, owner);
            if (m.Per.HasValue)
            {
                // Échelle d'archétype : phrase dédiée (« carte d'archétype Brûlure », la carte lancée comprise) pour la distinguer du statut Brûlure.
                string scaled = m.Per == ScaleKind.EquippedArchetypeCards
                    ? Loc.F(m.MaxCount == 1 ? "fx.mod_archetype_one" : "fx.mod_archetype", bonus, m.PerArchetype.HasValue ? Loc.T("archetype." + m.PerArchetype.Value) : "", m.MaxCount)
                    : Loc.F("fx.mod_scaled", bonus, ScaleText(m), m.MaxCount);
                return m.When != null ? Loc.F("fx.conditional", ConditionText(m.When), scaled) : scaled;
            }
            return m.When != null ? Loc.F("fx.mod_flat", bonus, ConditionText(m.When)) : Loc.F("fx.mod_always", bonus);
        }

        /// <summary>
        /// Réaction armée : « Riposte du lanceur (N de ses tours) : quand &lt;événement&gt; → &lt;effet&gt; &lt;cible&gt; » ; une carte à cible ZA ou 1A ajoute
        /// « seul le lanceur riposte » (la réaction n'est armée que sur lui). La cible s'écrit toujours. L'effet est toujours en % (jamais en montant brut).
        /// </summary>
        static string? ReactionLine(ReactionDef r, TargetRule? cardTarget)
        {
            string? effect = ReactionEffectLine(r.Effect);
            if (effect == null) return null;
            string trigger = Loc.F("reaction.trigger." + r.Trigger, r.Status == StatusType.None ? "" : Loc.StatusName(r.Status));
            bool solo = cardTarget == TargetRule.AllAllies || cardTarget == TargetRule.SingleAlly;
            return Loc.F(solo ? "reaction.line_solo" : "reaction.line", r.Turns, trigger, effect, ReactionTarget(r));
        }

        /// <summary>Effet d'une réaction : comme un effet de base, sauf le recul et l'élan, écrits sans « la cible » (la cible suit en fin de phrase).</summary>
        static string? ReactionEffectLine(EffectDef fx)
        {
            switch (fx.Type)
            {
                case EffectType.Advance: return Loc.F("fx.reaction_advance", fx.Value);
                case EffectType.PushBack: return Loc.F("fx.reaction_pushback", fx.Value);
                default: return BaseLine(fx, null);
            }
        }

        /// <summary>Cible d'une réaction, avec sa préposition (« au lanceur », « à tous les ennemis », « à l'attaquant »…) : l'unité concernée selon l'événement, sinon la règle et le choix.</summary>
        static string ReactionTarget(ReactionDef r)
        {
            if (r.OnTriggerUnit) return Loc.T("reaction.trigger_unit." + r.Trigger);
            switch (r.Target)
            {
                case TargetRule.SingleEnemy:
                case TargetRule.SingleAlly:
                    return Loc.F("reaction.to." + r.Target + "." + r.Pick, r.PickStatus == StatusType.None ? "" : Loc.StatusName(r.PickStatus));
                case TargetRule.AllEnemies:
                    return Loc.T(r.Pick == TargetPick.TwoRandom ? "reaction.to.AllEnemies.TwoRandom" : "reaction.to.AllEnemies");
                default:
                    return Loc.T("reaction.to." + r.Target);
            }
        }

        static string? BaseLine(EffectDef fx, SkillOwnerStats? owner)
        {
            switch (fx.Type)
            {
                case EffectType.Damage:
                    return owner.HasValue ? Loc.F("fx.damage_raw", owner.Value.Atk * fx.Value / 100) : Loc.F("fx.damage", fx.Value);
                case EffectType.Heal:
                    return owner.HasValue ? Loc.F("fx.heal_raw", owner.Value.MaxHp * fx.Value / 100) : Loc.F("fx.heal", fx.Value);
                case EffectType.Shield:
                    return owner.HasValue ? Loc.F("fx.shield_raw", owner.Value.MaxHp * fx.Value / 100) : Loc.F("fx.shield", fx.Value);
                case EffectType.PushBack: return Loc.F("fx.pushback", fx.Value);
                case EffectType.Advance: return Loc.F("fx.advance", fx.Value);
                case EffectType.GaugeDelta:
                    return fx.Value >= 0 ? Loc.F("fx.gauge_up", fx.Value / 10) : Loc.F("fx.gauge_down", -fx.Value / 10);
                case EffectType.Lifesteal: return Loc.F("fx.lifesteal", fx.Value);
                case EffectType.Purge: return fx.Value <= 0 ? Loc.T("fx.purge_all") : Loc.F("fx.purge", fx.Value);
                case EffectType.Bounce: return Loc.F("fx.bounce", fx.Value);
                case EffectType.Execute: return Loc.F("fx.execute", fx.Value, ExecuteThreshold);
                case EffectType.ArmReaction: return fx.Reaction == null ? null : ReactionLine(fx.Reaction, null);
                case EffectType.ConsumeDamage: return Loc.F(fx.Duration == 1 ? "fx.consume_damage_one" : "fx.consume_damage", Loc.StatusName(fx.Status), fx.Value, fx.Duration);
                case EffectType.ConsumeHeal: return Loc.F(fx.Duration == 1 ? "fx.consume_heal_one" : "fx.consume_heal", Loc.StatusName(fx.Status), fx.Value, fx.Duration);
                case EffectType.Spread: return Loc.F("fx.spread", Loc.StatusName(fx.Status));
                case EffectType.ApplyStatus when fx.Status == StatusType.Mark:
                    // La Marque dure jusqu'au prochain coup reçu : pas de durée affichée.
                    return fx.ChancePercent >= 100 ? Loc.F("fx.status_untimed", Loc.StatusName(fx.Status)) : Loc.F("fx.status_untimed_chance", Loc.StatusName(fx.Status), fx.ChancePercent);
                case EffectType.ApplyStatus:
                    int turns = fx.Duration >= 99 ? 1 : fx.Duration;
                    return fx.ChancePercent >= 100
                        ? Loc.F("fx.status", Loc.StatusName(fx.Status), turns)
                        : Loc.F("fx.status_chance", Loc.StatusName(fx.Status), turns, fx.ChancePercent);
                default: return null;
            }
        }
    }
}
