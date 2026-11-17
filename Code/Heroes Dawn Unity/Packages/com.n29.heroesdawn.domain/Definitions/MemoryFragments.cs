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
            new MemoryFragmentDef("mos_maglev_welcome", "mos_sergueiv", "zone_mos_1_1"),
            new MemoryFragmentDef("mos_monastery_register", "mos_sergueiv", "zone_mos_1_2"),
            new MemoryFragmentDef("mos_convoy_crates", "mos_sergueiv", "zone_mos_1_3"),
            new MemoryFragmentDef("mos_depot_passeur", "mos_sergueiv", "zone_mos_1_4"),
            // Chapitres suivants : ids du script narratif (13_Script_New_York.md, 14_Script_Moscou.md).
            new MemoryFragmentDef("ny_flushing_cameras", "ny_queens", "zone_ny_2_1"),
            new MemoryFragmentDef("ny_astoria_informant", "ny_queens", "zone_ny_2_2"),
            new MemoryFragmentDef("ny_jamaica_convoy", "ny_queens", "zone_ny_2_3"),
            new MemoryFragmentDef("ny_terminal_register", "ny_queens", "zone_ny_2_4"),
            new MemoryFragmentDef("ny_harlem_fire", "ny_harlem", "zone_ny_3_1"),
            new MemoryFragmentDef("ny_harlem_clinic", "ny_harlem", "zone_ny_3_2"),
            new MemoryFragmentDef("ny_harlem_market", "ny_harlem", "zone_ny_3_3"),
            new MemoryFragmentDef("ny_harlem_chalk", "ny_harlem", "zone_ny_3_4"),
            new MemoryFragmentDef("ny_times_square", "ny_manhattan", "zone_ny_4_1"),
            new MemoryFragmentDef("ny_midtown_list", "ny_manhattan", "zone_ny_4_2"),
            new MemoryFragmentDef("ny_un_stage", "ny_manhattan", "zone_ny_4_3"),
            new MemoryFragmentDef("ny_un_esplanade", "ny_manhattan", "zone_ny_4_4"),
            new MemoryFragmentDef("mos_refinery_leak", "mos_kapotnya", "zone_mos_2_1"),
            new MemoryFragmentDef("mos_convoy_residues", "mos_kapotnya", "zone_mos_2_2"),
            new MemoryFragmentDef("mos_lab_files", "mos_kapotnya", "zone_mos_2_3"),
            new MemoryFragmentDef("mos_quay_gromov", "mos_kapotnya", "zone_mos_2_4"),
            new MemoryFragmentDef("mos_towers_trophies", "mos_sadovoye", "zone_mos_3_1"),
            new MemoryFragmentDef("mos_auction", "mos_sadovoye", "zone_mos_3_2"),
            new MemoryFragmentDef("mos_holo_studio", "mos_sadovoye", "zone_mos_3_3"),
            new MemoryFragmentDef("mos_orlan_tower", "mos_sadovoye", "zone_mos_3_4"),
            new MemoryFragmentDef("mos_curfew", "mos_place_rouge", "zone_mos_4_1"),
            new MemoryFragmentDef("mos_kremlin_tunnels", "mos_place_rouge", "zone_mos_4_2"),
            new MemoryFragmentDef("mos_council_offer", "mos_place_rouge", "zone_mos_4_3"),
            new MemoryFragmentDef("mos_place_rouge_choice", "mos_place_rouge", "zone_mos_4_4"),
        };

        public static MemoryFragmentDef? Find(string id) => All.FirstOrDefault(f => f.Id == id);

        public static IReadOnlyList<MemoryFragmentDef> OfChapter(string chapterId) => All.Where(f => f.ChapterId == chapterId).ToList();
    }
}
