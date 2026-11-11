#nullable enable
namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>Signaux envoyés par l'interface au contexte musical (le hub vit dans un scope enfant, l'audio dans la racine).</summary>
    public interface IMusicSignals
    {
        /// <summary>Vrai tant que la page Boutique du hub est affichée.</summary>
        void SetShopOpen(bool open);
    }
}
