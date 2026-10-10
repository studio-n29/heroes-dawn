#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;

namespace N29.HeroesDawn.Domain.Board
{
    public enum IssueSeverity { Warning, Error }

    /// <summary>Un problème détecté sur un plateau (case concernée éventuelle) ; les erreurs empêchent de jouer la zone.</summary>
    public sealed class ZoneIssue
    {
        public IssueSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
        public CaseId? Case { get; }

        public ZoneIssue(IssueSeverity severity, string code, string message, CaseId? at = null)
        {
            Severity = severity; Code = code; Message = message; Case = at;
        }

        public override string ToString() => (Severity == IssueSeverity.Error ? "ERREUR " : "avertissement ") + Message;
    }

    /// <summary>
    /// Validation d'un plateau (01_Plateau, « Outils ») : graphe cohérent, connecté, objectifs atteignables, cases bien renseignées.
    /// Erreurs = injouable ; avertissements = écart avec les recommandations du GDD (impasses, embranchements, 3 objectifs…).
    /// </summary>
    public static class ZoneValidator
    {
        public static IReadOnlyList<ZoneIssue> Validate(ZoneDef zone, ContentCatalog content)
        {
            var issues = new List<ZoneIssue>();
            void Error(string code, string msg, CaseId? at = null) => issues.Add(new ZoneIssue(IssueSeverity.Error, code, msg, at));
            void Warn(string code, string msg, CaseId? at = null) => issues.Add(new ZoneIssue(IssueSeverity.Warning, code, msg, at));

            var ids = new HashSet<CaseId>();
            foreach (var n in zone.Nodes)
                if (!ids.Add(n.Id)) Error("node.duplicate", "Identifiant de case en double : " + n.Id.Value, n.Id);

            if (zone.Nodes.Count == 0) { Error("zone.empty", "Le plateau n'a aucune case."); return issues; }

            // ---- Entrée
            var entries = zone.Nodes.Where(n => n.Type == CaseType.Entry).ToList();
            if (!ids.Contains(zone.Entry)) Error("entry.missing", "La case d'entrée « " + zone.Entry.Value + " » n'existe pas.");
            else if (zone.Nodes.First(n => n.Id == zone.Entry).Type != CaseType.Entry) Error("entry.type", "La case d'entrée doit être de type Entrée.", zone.Entry);
            if (entries.Count != 1) Error("entry.count", "Il faut exactement une case Entrée (" + entries.Count + " trouvée(s)).");

            // ---- Positions
            foreach (var g in zone.Nodes.GroupBy(n => (n.X, n.Y)).Where(g => g.Count() > 1))
                Error("node.overlap", "Cases superposées en (" + g.Key.X + ", " + g.Key.Y + ") : " + string.Join(", ", g.Select(n => n.Id.Value)), g.First().Id);

            // ---- Liens
            var degree = ids.ToDictionary(i => i, _ => 0);
            var adj = ids.ToDictionary(i => i, _ => new List<CaseId>());
            var seen = new HashSet<(CaseId, CaseId)>();
            foreach (var l in zone.Links)
            {
                if (!ids.Contains(l.A) || !ids.Contains(l.B)) { Error("link.unknown", "Lien vers une case inconnue : " + l.A.Value + " — " + l.B.Value); continue; }
                if (l.A == l.B) { Error("link.self", "Lien d'une case vers elle-même : " + l.A.Value, l.A); continue; }
                var key = l.A.CompareTo(l.B) < 0 ? (l.A, l.B) : (l.B, l.A);
                if (!seen.Add(key)) { Error("link.duplicate", "Lien en double : " + l.A.Value + " — " + l.B.Value, l.A); continue; }
                degree[l.A]++; degree[l.B]++;
                adj[l.A].Add(l.B); adj[l.B].Add(l.A);
            }

            // ---- Connexité depuis l'entrée
            var reach = new HashSet<CaseId>();
            if (ids.Contains(zone.Entry))
            {
                var stack = new Stack<CaseId>();
                stack.Push(zone.Entry);
                reach.Add(zone.Entry);
                while (stack.Count > 0)
                    foreach (var n in adj[stack.Pop()]) if (reach.Add(n)) stack.Push(n);
                foreach (var id in ids.Where(i => !reach.Contains(i)).OrderBy(i => i))
                    Error("node.unreachable", "Case inaccessible depuis l'entrée : " + id.Value, id);
            }

            // ---- Recommandations du GDD
            foreach (var n in zone.Nodes.Where(n => n.Type != CaseType.Entry && degree[n.Id] == 1))
                Warn("node.deadend", "Impasse : la case " + n.Id.Value + " n'a qu'un seul lien (le GDD demande un plateau sans impasse).", n.Id);
            int branches = degree.Values.Count(d => d >= 3);
            if (branches < 3) Warn("zone.branches", "Seulement " + branches + " embranchement(s) ; le GDD en demande au moins 3.");
            if (zone.Objectives.Count != 3) Warn("zone.objectives", "La zone a " + zone.Objectives.Count + " objectif(s) ; le GDD en prévoit 3.");
            if (zone.Nodes.Count < 15 || zone.Nodes.Count > 35) Warn("zone.size", "La zone a " + zone.Nodes.Count + " cases ; le GDD vise environ 25.");

            // ---- Cases
            foreach (var n in zone.Nodes)
            {
                switch (n.Type)
                {
                    case CaseType.Combat:
                        if (n.Combat != null)
                        {
                            if (n.Combat.EnemyTable.Count == 0) Error("combat.table", "Combat sans ennemi dans sa table : " + n.Id.Value, n.Id);
                            foreach (var e in n.Combat.EnemyTable.Distinct())
                                if (!content.Enemies.Any(d => d.Id == e)) Error("combat.enemy", "Ennemi inconnu « " + e.Value + " » dans la table de " + n.Id.Value, n.Id);
                        }
                        else if (zone.Encounters.Count == 0) Error("combat.encounter", "Aucune rencontre disponible pour le combat " + n.Id.Value + " (ni table, ni rencontre de zone).", n.Id);
                        if (n.Rank == CombatRank.Elite && !n.IsFixed) Warn("combat.elite", "Un combat Élite devrait être une case fixe : " + n.Id.Value, n.Id);
                        break;
                    case CaseType.MiniGame:
                        if (n.Activity.HasValue && !content.HasActivity(n.Activity.Value))
                            Error("activity.unknown", "Activité inconnue « " + n.Activity.Value.Value + " » sur la case " + n.Id.Value, n.Id);
                        break;
                }
            }

            // ---- Objectifs
            var objectiveIds = new HashSet<string>();
            foreach (var o in zone.Objectives)
            {
                if (!objectiveIds.Add(o.Id)) Error("objective.duplicate", "Objectif en double : " + o.Id);
                if (!ids.Contains(o.Target)) { Error("objective.target", "L'objectif « " + o.Id + " » vise une case inconnue : " + o.Target.Value); continue; }
                var target = zone.Nodes.First(n => n.Id == o.Target);
                if (reach.Count > 0 && !reach.Contains(o.Target)) Error("objective.unreachable", "L'objectif « " + o.Id + " » vise une case inaccessible.", o.Target);
                if (!target.IsFixed) Warn("objective.fixed", "La case de l'objectif « " + o.Id + " » devrait être fixe.", o.Target);
                var want = o.Type == ObjectiveType.DefeatCombat ? (CaseType?)CaseType.Combat
                         : o.Type == ObjectiveType.CompleteMiniGame ? CaseType.MiniGame
                         : o.Type == ObjectiveType.ResolveNarrative ? CaseType.Narrative : null;
                if (want.HasValue && target.Type != want.Value)
                    Error("objective.type", "L'objectif « " + o.Id + " » (" + o.Type + ") demande une case " + want.Value + ", pas " + target.Type + ".", o.Target);
            }
            if (!zone.Objectives.Any(o => o.Type == ObjectiveType.DefeatCombat)) Warn("zone.keycombat", "Aucun objectif « vaincre un combat clé » (le GDD en demande au moins un).");
            return issues;
        }

        public static bool IsPlayable(IReadOnlyList<ZoneIssue> issues) => issues.All(i => i.Severity != IssueSeverity.Error);
    }
}
