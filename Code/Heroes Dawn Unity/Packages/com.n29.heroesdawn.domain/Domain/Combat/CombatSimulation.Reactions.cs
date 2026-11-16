#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    /// <summary>Réaction armée sur une unité : sa définition, ses tours restants (tours du porteur) et son rang d'armement dans le combat.</summary>
    public sealed class ReactionInstance
    {
        public ReactionDef Def { get; }
        /// <summary>Tours restants, décrémentés au début de chaque tour du porteur ; remis à <see cref="ReactionDef.Turns"/> au réarmement.</summary>
        public int TurnsLeft { get; set; }
        /// <summary>Rang d'armement dans le combat (croissant) : départage les réactions d'un même porteur.</summary>
        public int Order { get; }
        /// <summary>Indice, dans la liste d'événements du pas en cours, à partir duquel la réaction écoute (armée en cours de pas) ; 0 ensuite.</summary>
        internal int ListensFrom { get; set; }

        public ReactionInstance(ReactionDef def, int turnsLeft, int order) { Def = def; TurnsLeft = turnsLeft; Order = order; }
    }

    /// <summary>
    /// Réactions (P2). Après chaque pas de frise (et chaque série de commandes appliquées hors frise), les événements produits par ce pas sont lus
    /// dans l'ordre ; pour chacun, les réactions des porteurs vivants partent par numéro d'unité croissant puis par ordre d'armement, au plus une fois
    /// par événement. Les événements produits par les réactions ne sont jamais relus (file, pas de récursion). Sans aucune réaction armée dans le
    /// combat, rien n'est fait : ni travail, ni tirage, ni événement.
    /// Précisions : « allié » compte le porteur (un allié reçoit un statut) ; une réaction n'émet aucun événement en expirant (lire <see cref="CombatUnit.Reactions"/>) ;
    /// <see cref="ReactionArmedEvent"/> donne le porteur et le déclencheur, pas la définition ; les vagues ne sont pas séparées : une réaction armée
    /// reste armée à la vague suivante, et une réaction « un ennemi est vaincu → … sur ZE » qui part sur le dernier ennemi d'une vague frappe la vague
    /// suivante, déjà apparue quand les réactions du pas sont traitées.
    /// </summary>
    public sealed partial class CombatSimulation
    {
        /// <summary>Vrai dès qu'une réaction a été armée dans ce combat ; faux : le traitement des réactions est sauté.</summary>
        bool _reactionsArmed;
        int _reactionOrder;
        /// <summary>Auteur de chaque coup du pas en cours (tenu seulement quand une réaction est armée), vidé après le traitement.</summary>
        readonly Dictionary<CombatEvent, int> _hitBy = new Dictionary<CombatEvent, int>();

        /// <summary>
        /// Effets <see cref="EffectType.ArmReaction"/> d'un lancer : traités une seule fois, APRÈS la boucle des cibles, sur le lanceur, quels que soient
        /// le nombre de cibles, leur mort ou un raté. La réaction n'entend donc pas les effets de la carte qui l'arme (elle écoute à partir de son armement).
        /// Chance : un seul tirage, et seulement si elle est sous 100 % (<c>Chance(100)</c> ne tire pas). Condition : évaluée sur le lanceur (lanceur = cible).
        /// Sans ArmReaction dans la liste : aucun travail ni tirage.
        /// </summary>
        void ArmReactions(CombatUnit actor, IReadOnlyList<EffectDef> effects, List<CombatEvent> output)
        {
            foreach (var fx in effects)
            {
                if (fx.Type != EffectType.ArmReaction || fx.Reaction == null || !actor.IsAlive) continue;
                if (fx.When != null && !Holds(fx.When, actor, actor)) continue;
                if (!_rng.Chance(fx.ChancePercent)) continue;
                ArmReaction(actor, fx.Reaction, output);
            }
        }

        /// <summary>Arme (ou réarme : durée remise à Turns, même rang, sans doublon) la réaction sur le porteur.</summary>
        void ArmReaction(CombatUnit carrier, ReactionDef def, List<CombatEvent> output)
        {
            _reactionsArmed = true;
            ReactionInstance? existing = null;
            foreach (var r in carrier.Reactions) if (ReferenceEquals(r.Def, def)) { existing = r; break; }
            if (existing != null) existing.TurnsLeft = def.Turns;
            else carrier.Reactions.Add(new ReactionInstance(def, def.Turns, _reactionOrder++) { ListensFrom = output.Count });
            output.Add(new ReactionArmedEvent(Tick, carrier.Id, def.Trigger, def.Turns));
        }

        /// <summary>Début de tour du porteur : chaque réaction perd un tour et disparaît à 0.</summary>
        void TickReactions(CombatUnit u)
        {
            for (int i = u.Reactions.Count - 1; i >= 0; i--)
                if (--u.Reactions[i].TurnsLeft <= 0) u.Reactions.RemoveAt(i);
        }

        /// <summary>Retient l'auteur du coup dont le DamageEvent vient d'être ajouté à partir de <paramref name="from"/>.</summary>
        void NoteHitter(List<CombatEvent> output, int from, int actor)
        {
            for (int i = from; i < output.Count; i++)
                if (output[i] is DamageEvent) { _hitBy[output[i]] = actor; return; }
        }

        /// <summary>Déclenche les réactions sur les événements <paramref name="output"/>[<paramref name="fromIndex"/>..] présents à l'appel (voir le résumé de la classe).</summary>
        internal void ProcessReactions(List<CombatEvent> output, int fromIndex)
        {
            if (!_reactionsArmed) return;
            int end = output.Count;
            bool fired = false;
            for (int i = fromIndex; i < end && !IsOver; i++)
            {
                var ev = output[i];
                for (int c = 0; c < _units.Count; c++) // les unités sont rangées par numéro croissant
                {
                    var carrier = _units[c];
                    var list = carrier.Reactions;
                    for (int k = 0; k < list.Count && carrier.IsAlive; k++)
                    {
                        var r = list[k];
                        if (i < r.ListensFrom || !Triggers(r.Def, carrier, ev, out int unit)) continue;
                        fired |= FireReaction(carrier, r.Def, unit, output);
                    }
                }
            }
            foreach (var u in _units) foreach (var r in u.Reactions) r.ListensFrom = 0;
            _hitBy.Clear();
            if (fired) CheckEnd(output); // une réaction peut vaincre le dernier ennemi ou allié
        }

        /// <summary>Vrai si l'événement déclenche la réaction de ce porteur ; <paramref name="unit"/> : l'unité concernée (receveur du statut, unité vaincue, attaquant du porteur, ennemi touché par son critique ; -1 si inconnue).</summary>
        bool Triggers(ReactionDef d, CombatUnit carrier, CombatEvent ev, out int unit)
        {
            unit = -1;
            switch (d.Trigger)
            {
                case ReactionTrigger.EnemyReceivesStatus:
                case ReactionTrigger.AllyReceivesStatus:
                {
                    if (!(ev is StatusAppliedEvent s) || s.Status != d.Status) return false;
                    bool ally = _units[s.Target].Side == carrier.Side;
                    if (ally != (d.Trigger == ReactionTrigger.AllyReceivesStatus)) return false;
                    unit = s.Target;
                    return true;
                }
                case ReactionTrigger.EnemyDefeated:
                case ReactionTrigger.AllyDefeated:
                {
                    if (!(ev is UnitDefeatedEvent v) || v.Unit == carrier.Id) return false;
                    bool ally = _units[v.Unit].Side == carrier.Side;
                    if (ally != (d.Trigger == ReactionTrigger.AllyDefeated)) return false;
                    unit = v.Unit;
                    return true;
                }
                case ReactionTrigger.CarrierHit:
                {
                    // Seulement un coup dont l'auteur est connu (ennemi ou allié) : jamais la brûlure, qui mord à chaque point de frise.
                    if (!(ev is DamageEvent h) || h.Target != carrier.Id || !_hitBy.TryGetValue(h, out int attacker)) return false;
                    unit = attacker;
                    return true;
                }
                case ReactionTrigger.CarrierCrit:
                {
                    if (!(ev is DamageEvent h) || !h.Crit || !_hitBy.TryGetValue(h, out int attacker) || attacker != carrier.Id) return false;
                    unit = h.Target;
                    return true;
                }
                default: return false;
            }
        }

        /// <summary>
        /// Applique l'effet de la réaction (valeur fixe, porteur comme lanceur : critique et défense s'appliquent). Cible « déclencheur » : l'unité concernée
        /// si elle est vivante ; sinon la règle nommée, comme une compétence du porteur (un choix aléatoire tire une fois). Sans cible : rien, sans tirage.
        /// </summary>
        bool FireReaction(CombatUnit carrier, ReactionDef d, int unit, List<CombatEvent> output)
        {
            if (d.Effect.Type == EffectType.ArmReaction) return false; // une réaction n'en arme pas d'autre
            List<int> targets;
            if (d.OnTriggerUnit)
            {
                targets = new List<int>();
                if (unit >= 0 && _units[unit].IsAlive) targets.Add(unit);
            }
            else targets = ResolvePick(carrier, d.Target, d.Pick, d.PickStatus) ?? ResolveTargets(carrier, d.Target, -1);
            if (targets.Count == 0) return false;
            output.Add(new ReactionTriggeredEvent(Tick, carrier.Id, d.Trigger, unit, targets.ToArray()));
            ApplyEffects(carrier, new[] { d.Effect }, targets, output);
            return true;
        }
    }
}
