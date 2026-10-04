#nullable enable
namespace N29.HeroesDawn.Presentation.Wip
{
    /// <summary>Fonction affichée mais pas encore développée : identifiant, titre et texte « ce qui est prévu ».</summary>
    public sealed class WipFeature
    {
        public string Id { get; }
        public string TitleKey => "wip." + Id + ".title";
        public string PlannedKey => "wip." + Id + ".planned";

        public WipFeature(string id) => Id = id;
    }
}
