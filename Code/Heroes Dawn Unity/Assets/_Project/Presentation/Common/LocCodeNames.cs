#nullable enable
using System.Collections.Generic;

namespace N29.HeroesDawn.Presentation.Common
{
    /// <summary>Noms de code des héros : identiques dans toutes les langues (les autres textes passent par les dictionnaires de langue).</summary>
    public static partial class Loc
    {
        public static readonly IReadOnlyDictionary<string, string> CodeNames = new Dictionary<string, string>
        {
            ["ancre"] = "Anchor", ["verre"] = "Glass", ["givre"] = "Frost", ["echo"] = "Echo",
            ["arc"] = "Arc", ["rempart"] = "Bulwark", ["seve"] = "Sap", ["etincelle"] = "Spark",
        };
    }
}
