#nullable enable
namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Hook de voix des fragments de souvenirs : dit si un fichier audio existe pour la clé d'un fragment. Les voix arrivent avec L14.</summary>
    public interface IFragmentAudio
    {
        bool Has(string audioKey);
    }

    /// <summary>Aucune voix fournie : chaque fragment affiche son texte et « audio à venir ».</summary>
    public sealed class NoFragmentAudio : IFragmentAudio
    {
        public bool Has(string audioKey) => false;
    }
}
