#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Board;

namespace N29.HeroesDawn.Application.Features.Zone
{
    /// <summary>
    /// Déblocage des agents hors boutique : fin d'un arc narratif (zone validée), événement temporaire (zone validée un jour d'événement)
    /// et prouesse (zone validée en accomplissant l'exploit : un seul agent au combat, ou aucune défaite).
    /// </summary>
    public static class AgentUnlocks
    {
        /// <summary>Agents qui rejoignent le joueur à la validation de <paramref name="zone"/> ; ils sont ajoutés à l'effectif (niveau de recrue, hors escouade).</summary>
        public static IReadOnlyList<MemberDef> OnZoneValidated(GameSession s, ZoneRun? run, ZoneId zone, DateTime localNow)
        {
            var joined = new List<MemberDef>();
            foreach (var def in s.Content.Members)
            {
                var u = def.Unlock;
                if (u == null || u.Kind == UnlockKind.Shop || s.Owns(def.Id)) continue;
                bool ok;
                switch (u.Kind)
                {
                    case UnlockKind.Arc: ok = u.Zone.HasValue && s.CompletedZones.Contains(u.Zone.Value.Value); break;
                    case UnlockKind.Event: ok = u.Zone == zone && u.IsEventDay(localNow); break;
                    case UnlockKind.Feat: ok = u.Zone == zone && FeatMet(s, run, u.Feat); break;
                    default: ok = false; break;
                }
                if (!ok) continue;
                s.Roster.Add(new TeamMember(def, MetaRules.RecruitLevel));
                joined.Add(def);
            }
            return joined;
        }

        /// <summary>Rattrape les arcs déjà accomplis (sauvegarde ancienne, ou nouvel agent ajouté au contenu) : leurs agents rejoignent l'effectif.</summary>
        public static void CatchUpArcs(GameSession s)
        {
            foreach (var def in s.Content.Members)
            {
                var u = def.Unlock;
                if (u == null || u.Kind != UnlockKind.Arc || s.Owns(def.Id) || !u.Zone.HasValue) continue;
                if (s.CompletedZones.Contains(u.Zone.Value.Value)) s.Roster.Add(new TeamMember(def, MetaRules.RecruitLevel));
            }
        }

        static bool FeatMet(GameSession s, ZoneRun? run, FeatKind feat)
        {
            if (run == null) return false;
            bool fought = run.Log.Any(a => a.Type == BoardActionType.CombatResult);
            switch (feat)
            {
                case FeatKind.SoloClear: return fought && s.RunEngaged.Count == 1;
                case FeatKind.NoDefeat: return fought && !run.Log.Any(a => a.Type == BoardActionType.CombatResult && a.Value == 0);
                default: return false;
            }
        }
    }
}
