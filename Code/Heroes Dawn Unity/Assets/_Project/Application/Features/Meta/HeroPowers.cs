#nullable enable
using System.Collections.Generic;
using System.Linq;

namespace N29.HeroesDawn.Application.Features.Meta
{
    /// <summary>Un pouvoir proposé sur la fiche d'identification du joueur (profils de <c>02_Lore_Narration/02_Profils_Pouvoirs.md</c>).</summary>
    public sealed class HeroPower
    {
        public string Id { get; }
        public string FamilyId { get; }
        public HeroPower(string id, string familyId) { Id = id; FamilyId = familyId; }
        public string NameKey => "power." + Id + ".name";
    }

    /// <summary>Une des quatre familles de pouvoirs du jeu ; regroupe les pouvoirs du tableau de la fiche.</summary>
    public sealed class HeroPowerFamily
    {
        public string Id { get; }
        public IReadOnlyList<HeroPower> Powers { get; }
        public HeroPowerFamily(string id, IReadOnlyList<HeroPower> powers) { Id = id; Powers = powers; }
        public string NameKey => "power.family." + Id + ".name";
    }

    /// <summary>
    /// Catalogue des pouvoirs que le joueur peut inscrire sur sa fiche. Quatre familles alignées sur le lore (Matière, Élémentaire, Physique/Bio, Perception),
    /// beaucoup de choix dans chacune. Le pouvoir est un trait de personnage (récit et profil) : il n'ajoute pas d'unité de combat.
    /// La sauvegarde ne garde que l'identifiant : ne jamais renommer un identifiant existant.
    /// </summary>
    public static class HeroPowers
    {
        public static readonly IReadOnlyList<HeroPowerFamily> Families = new[]
        {
            Family("matiere", "ferromodelage", "lithomodelage", "cristallisation", "polymorphie_partielle", "materialisation", "transmutation", "polymorphie_totale"),
            Family("elementaire", "electrogenese", "pyrochimie", "cryogenie", "liquide_corrosif", "decharges_electriques", "vapeur", "magnetisme"),
            Family("physique_bio", "force_colossale", "insensibilite_douleur", "hyper_regeneration", "adaptation_sous_marine", "detoxification", "adherence", "thermoregulation"),
            Family("perception", "camouflage_optique", "bullet_time", "echolocation", "vigilance_continue", "magnetoreception", "vision_thermique", "precognition"),
        };

        public static readonly IReadOnlyList<HeroPower> All = Families.SelectMany(f => f.Powers).ToList();

        public static HeroPower? Find(string? id) => All.FirstOrDefault(p => p.Id == id);

        static HeroPowerFamily Family(string id, params string[] powers) =>
            new HeroPowerFamily(id, powers.Select(p => new HeroPower(p, id)).ToList());
    }
}
