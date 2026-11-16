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
        /// pour les choix à cible unique. Une liste vide signifie « personne à viser » (aucun tirage n'a eu lieu).
        /// </summary>
        List<int>? ResolvePick(CombatUnit actor, SkillDef skill)
        {
            var pick = skill.Pick;
            if (pick == TargetPick.Default) return null;
            Side foe = actor.Side == Side.Ally ? Side.Enemy : Side.Ally;
            var list = new List<int>();
            switch (skill.Target)
            {
                case TargetRule.SingleEnemy:
                {
                    if (pick == TargetPick.TwoRandom) return null;
                    foreach (var u in _units) if (u.Side == foe && u.IsAlive && u.Has(StatusType.Taunt)) { list.Add(u.Id); return list; }
                    int t = PickOne(foe, pick, skill.PickStatus, false);
                    if (t >= 0) list.Add(t);
                    return list;
                }
                case TargetRule.SingleAlly:
                {
                    if (pick == TargetPick.TwoRandom) return null;
                    int t = PickOne(actor.Side, pick, skill.PickStatus, true);
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

        List<int> LivingIds(Side side)
        {
            var ids = new List<int>();
            foreach (var u in _units) if (u.Side == side && u.IsAlive) ids.Add(u.Id);
            return ids;
        }

        /// <summary>Une cible d'un camp selon le choix ; -1 s'il n'y a personne (sans tirage). Égalités : plus petit identifiant.</summary>
        int PickOne(Side side, TargetPick pick, StatusType status, bool ally)
        {
            switch (pick)
            {
                case TargetPick.LowestHp: return LowestHpRatio(side);
                case TargetPick.HighestHp: return HighestHpRatio(side);
                case TargetPick.NextToAct: return NextToActOf(side);
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

        /// <summary>Le vivant du camp le plus proche de son point d'action : plus grand rapport Point / ActionPoint, comparé en produits croisés.</summary>
        int NextToActOf(Side side)
        {
            CombatUnit? best = null;
            foreach (var u in _units)
            {
                if (u.Side != side || !u.IsAlive) continue;
                if (best == null || (long)u.Point * best.ActionPoint > (long)best.Point * u.ActionPoint) best = u;
            }
            return best == null ? -1 : best.Id;
        }
    }
}
