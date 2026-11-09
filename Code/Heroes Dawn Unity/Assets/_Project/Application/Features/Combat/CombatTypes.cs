#nullable enable
using System.Collections.Generic;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Combat
{
    public static class CombatErrors
    {
        public static readonly DomainError NoPendingCombat = new DomainError("combat.none", "errors.no_combat");
        public static readonly DomainError NoFighters = new DomainError("combat.no_fighters", "errors.squad_exhausted");
    }

    public readonly struct FatigueChange
    {
        public readonly MemberId Member;
        public readonly int Before;
        public readonly int After;
        /// <summary>EXP gagnée par ce combat et EXP cumulée après (0 en cas de défaite).</summary>
        public readonly int XpGain;
        public readonly int XpTotal;
        public FatigueChange(MemberId member, int before, int after, int xpGain = 0, int xpTotal = 0)
        { Member = member; Before = before; After = after; XpGain = xpGain; XpTotal = xpTotal; }
    }

    public sealed class CombatReport
    {
        public bool Victory { get; }
        public int Credits { get; }
        public CombatRank Rank { get; }
        public bool Boss { get; }
        public IReadOnlyList<FatigueChange> Fatigue { get; }
        public IReadOnlyList<BoardEvent> BoardEvents { get; }
        public bool ReturnedToHq { get; }
        /// <summary>Objet laissé par ce combat gagné (aucun : null).</summary>
        public ItemId? Drop { get; }
        /// <summary>Scraps et recette laissés par ce combat gagné pour le labo (défaite : null).</summary>
        public CraftDrops? Crafting { get; }

        public CombatReport(bool victory, int credits, CombatRank rank, bool boss, IReadOnlyList<FatigueChange> fatigue,
                            IReadOnlyList<BoardEvent> boardEvents, bool returnedToHq, ItemId? drop = null, CraftDrops? crafting = null)
        {
            Victory = victory; Credits = credits; Rank = rank; Boss = boss; Fatigue = fatigue; BoardEvents = boardEvents; ReturnedToHq = returnedToHq; Drop = drop; Crafting = crafting;
        }
    }
}
