#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Definitions
{
    /// <summary>
    /// Fragment de souvenir : un court récit (texte et, plus tard, voix) sur un événement du lore, gagné en validant une zone
    /// et relu depuis la page Chapitres. Titre et texte : <c>fragment.&lt;id&gt;.title</c> et <c>fragment.&lt;id&gt;.text</c>.
    /// </summary>
    public sealed class MemoryFragmentDef
    {
        public string Id { get; }
        /// <summary>Chapitre sous lequel le fragment est rangé.</summary>
        public string ChapterId { get; }
        /// <summary>Zone dont la validation débloque le fragment.</summary>
        public string UnlockZone { get; }
        public string TitleKey => "fragment." + Id + ".title";
        public string TextKey => "fragment." + Id + ".text";
        /// <summary>Clé du fichier audio (hook de voix) : le fichier est facultatif, sans lui le texte seul est lu.</summary>
        public string AudioKey => "fragments/" + Id;

        public MemoryFragmentDef(string id, string chapterId, string unlockZone)
        {
            Id = id; ChapterId = chapterId; UnlockZone = unlockZone;
        }
    }

    public static class MemoryFragments
    {
        public static readonly IReadOnlyList<MemoryFragmentDef> All = new[]
        {
            new MemoryFragmentDef("redhook_docks", "ny_brooklyn", "zone_ny_1_1"),
            new MemoryFragmentDef("brownsville_hostages", "ny_brooklyn", "zone_ny_1_2"),
            new MemoryFragmentDef("coney_trap", "ny_brooklyn", "zone_ny_1_3"),
            new MemoryFragmentDef("dumbo_bridge", "ny_brooklyn", "zone_ny_1_4"),
        };

        public static MemoryFragmentDef? Find(string id) => All.FirstOrDefault(f => f.Id == id);

        public static IReadOnlyList<MemoryFragmentDef> OfChapter(string chapterId) => All.Where(f => f.ChapterId == chapterId).ToList();
    }
}
