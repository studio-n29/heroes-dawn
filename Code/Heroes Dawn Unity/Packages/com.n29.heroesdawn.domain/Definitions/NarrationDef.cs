#nullable enable
using System;
using System.Collections.Generic;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>Moment d'un dialogue de narration par rapport à l'action d'une case.</summary>
    public enum NarrationPhase { Before, After }

    /// <summary>Réplique : qui parle (identifiant d'un agent, ou d'un personnage non joueur) et la clé de son texte.</summary>
    public sealed class NarrationLineDef
    {
        public string Speaker { get; }
        public string TextKey { get; }
        public NarrationLineDef(string speaker, string textKey) { Speaker = speaker; TextKey = textKey; }
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
