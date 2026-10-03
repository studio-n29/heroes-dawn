using System.Collections.Generic;

namespace N29.HeroesDawn.EditorTools.Prefabs
{
    public enum OverwritePolicy { Never, All, Selected }

    /// <summary>Résultat d'une génération : chemins relatifs créés, écrasés, conservés, et échecs (chemin : erreur).</summary>
    public sealed class GenerationReport
    {
        public readonly List<string> Created = new List<string>();
        public readonly List<string> Overwritten = new List<string>();
        public readonly List<string> Skipped = new List<string>();
        public readonly List<string> Failed = new List<string>();

        public override string ToString()
            => $"créés {Created.Count}, écrasés {Overwritten.Count}, conservés {Skipped.Count}, échecs {Failed.Count}"
               + (Failed.Count > 0 ? "\n  " + string.Join("\n  ", Failed) : "");
    }
}
