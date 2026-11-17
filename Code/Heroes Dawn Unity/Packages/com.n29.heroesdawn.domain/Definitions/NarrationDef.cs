#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Moment d'un dialogue de narration par rapport à l'action d'une case.</summary>
    public enum NarrationPhase { Before, After }

    /// <summary>
    /// Réplique : qui parle (identifiant d'un agent, ou d'un personnage non joueur) et la clé de son texte.
    /// <see cref="Route"/> : parcours pour lequel elle est jouée (<c>agent_d</c>, <c>agent_m</c>) ; nul : tous les parcours.
    /// <see cref="Replaces"/> : clé de texte de la réplique de base qu'elle remplace pour ce parcours (variante .d / .m du script) ; nul : réplique ajoutée.
    /// </summary>
    public sealed class NarrationLineDef
    {
        public string Speaker { get; }
        public string TextKey { get; }
        public string? Route { get; }
        public string? Replaces { get; }
        public NarrationLineDef(string speaker, string textKey, string? route = null, string? replaces = null)
        { Speaker = speaker; TextKey = textKey; Route = route; Replaces = replaces; }

        /// <summary>Répliques jouées pour ce parcours, dans l'ordre : celles d'un autre parcours sont écartées, une réplique de base remplacée par une variante du parcours cède sa place.</summary>
        public static IReadOnlyList<NarrationLineDef> ForRoute(IReadOnlyList<NarrationLineDef> lines, string? hero)
        {
            var result = new List<NarrationLineDef>();
            foreach (var l in lines)
            {
                if (l.Route != null && l.Route != hero) continue;
                bool replaced = false;
                foreach (var v in lines) if (v.Route != null && v.Route == hero && v.Replaces == l.TextKey) { replaced = true; break; }
                if (!replaced) result.Add(l);
            }
            return result;
        }
    }

    /// <summary>Dialogues d'une case : avant l'action (introduction) et/ou après sa résolution (conclusion).</summary>
    public sealed class NarrationDef
    {
        public IReadOnlyList<NarrationLineDef> Before { get; }
        public IReadOnlyList<NarrationLineDef> After { get; }

        public NarrationDef(IReadOnlyList<NarrationLineDef>? before, IReadOnlyList<NarrationLineDef>? after)
        {
            Before = before ?? Array.Empty<NarrationLineDef>();
            After = after ?? Array.Empty<NarrationLineDef>();
        }

        public IReadOnlyList<NarrationLineDef> Of(NarrationPhase phase) => phase == NarrationPhase.Before ? Before : After;
        public bool IsEmpty => Before.Count == 0 && After.Count == 0;
    }
}
