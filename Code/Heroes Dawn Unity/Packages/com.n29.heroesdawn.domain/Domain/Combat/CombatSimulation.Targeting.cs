#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Combat
{
    public sealed partial class CombatSimulation
    {
        /// <summary>
        /// Ciblage fin (<see cref="SkillDef.Pick"/>). Renvoie null quand il ne s'applique pas (Pick par défaut, ou règle incompatible) :
        /// l'appelant suit alors le chemin historique sans aucun tirage. La cible voulue est ignorée ; la Provocation d'un ennemi prime
        /// pour les choix à cible unique. Une liste vide signifie « personne à viser » (aucun tirage n'a eu lieu). Côté allié, NextToAct écarte le lanceur et les alliés déjà prêts.
        /// </summary>
        List<int>? ResolvePick(CombatUnit actor, SkillDef skill) => ResolvePick(actor, skill.Target, skill.Pick, skill.PickStatus);

        /// <summary>Même ciblage fin pour une règle et un choix donnés (cible nommée d'une réaction, lancée par son porteur).</summary>
        List<int>? ResolvePick(CombatUnit actor, TargetRule target, TargetPick pick, StatusType pickStatus)
        {
            if (pick == TargetPick.Default) return null;
            Side foe = actor.Side == Side.Ally ? Side.Enemy : Side.Ally;
            var list = new List<int>();
            switch (target)
            {
                case TargetRule.SingleEnemy:
                {
                    if (pick == TargetPick.TwoRandom) return null;
                    foreach (var u in _units) if (u.Side == foe && u.IsAlive && u.Has(StatusType.Taunt)) { list.Add(u.Id); return list; }
                    int t = PickOne(foe, pick, pickStatus, false);
                    if (t >= 0) list.Add(t);
                    return list;
                }
                case TargetRule.SingleAlly:
                {
                    if (pick == TargetPick.TwoRandom) return null;
                    int t = PickOne(actor.Side, pick, pickStatus, true, actor.Id);
                    if (t >= 0) list.Add(t);
                    return list;
                }
                case TargetRule.AllEnemies:
                {
                    if (pick != TargetPick.TwoRandom) return null;
                    var pool = LivingIds(foe);
                    if (pool.Count == 0) return list;
                    // Deux tirages successifs sans remise ; un seul ennemi vivant : un seul tirage, le second est sauté.
                    int first = pool[_rng.Range(0, pool.Count)];
                    pool.Remove(first);
                    list.Add(first);
                    if (pool.Count > 0) list.Add(pool[_rng.Range(0, pool.Count)]);
                    list.Sort();
                    return list;
                }
                default:
                    return null;
            }
        }

        /// <summary>
        /// Vrai si <see cref="ResolvePick"/> trouverait au moins une cible, sans aucun tirage (l'IA ne choisit pas une compétence qui partirait dans le vide).
        /// Pick par défaut, règle ou choix que Pick n'affecte pas : vrai (chemin historique inchangé).
        /// </summary>
        bool PickHasTarget(CombatUnit actor, SkillDef skill)
        {
            if (skill.Pick == TargetPick.Default) return true;
            Side foe = actor.Side == Side.Ally ? Side.Enemy : Side.Ally;
            switch (skill.Target)
            {
                case TargetRule.SingleEnemy:
                    return skill.Pick == TargetPick.TwoRandom || LivingIds(foe).Count > 0;
                case TargetRule.SingleAlly:
                    if (skill.Pick == TargetPick.TwoRandom) return true;
                    return skill.Pick == TargetPick.NextToAct ? NextToActOf(actor.Side, actor.Id, true) >= 0 : LivingIds(actor.Side).Count > 0;
                case TargetRule.AllEnemies:
                    return skill.Pick != TargetPick.TwoRandom || LivingIds(foe).Count > 0;
                default:
                    return true;
            }
        }

        List<int> LivingIds(Side side)
        {
            var ids = new List<int>();
            foreach (var u in _units) if (u.Side == side && u.IsAlive) ids.Add(u.Id);
            return ids;
        }

        /// <summary>Une cible d'un camp selon le choix ; -1 s'il n'y a personne (sans tirage). Égalités : plus petit identifiant.</summary>
        int PickOne(Side side, TargetPick pick, StatusType status, bool ally, int except = -1)
        {
            switch (pick)
            {
                case TargetPick.LowestHp: return LowestHpRatio(side);
                case TargetPick.HighestHp: return HighestHpRatio(side);
                case TargetPick.NextToAct: return NextToActOf(side, except, ally);
                case TargetPick.StatusHolder:
                {
                    foreach (var u in _units) if (u.Side == side && u.IsAlive && u.Has(status)) return u.Id;
                    if (ally) return LowestHpRatio(side);
                    foreach (var u in _units) if (u.Side == side && u.IsAlive) return u.Id;
                    return -1;
                }
                case TargetPick.Random:
                {
                    var pool = LivingIds(side);
                    return pool.Count == 0 ? -1 : pool[_rng.Range(0, pool.Count)];
                }
                default: return -1;
            }
        }

        int HighestHpRatio(Side side)
        {
            int best = -1;
            long bestRatio = -1;
            foreach (var u in _units)
            {
                if (u.Side != side || !u.IsAlive) continue;
                long r = (long)u.Hp * 1000 / u.MaxHp;
                if (r > bestRatio) { bestRatio = r; best = u.Id; }
            }
            return best;
        }

        /// <summary>Le vivant du camp le plus proche de son point d'action : plus grand rapport Point / ActionPoint, comparé en produits croisés. <paramref name="except"/> : unité écartée (le lanceur d'une carte « 1A:prochain », qui agit déjà). <paramref name="skipReady"/> : écarte aussi les unités déjà prêtes (côté allié : Advance et Gauge + ne font rien sur elles).</summary>
        int NextToActOf(Side side, int except = -1, bool skipReady = false)
        {
            CombatUnit? best = null;
            foreach (var u in _units)
            {
                if (u.Side != side || !u.IsAlive || u.Id == except || (skipReady && u.IsReady)) continue;
                if (best == null || (long)u.Point * best.ActionPoint > (long)best.Point * u.ActionPoint) best = u;
            }
            return best == null ? -1 : best.Id;
        }
    }
}
