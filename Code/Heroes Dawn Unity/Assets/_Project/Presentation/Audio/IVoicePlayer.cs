#nullable enable
namespace N29.HeroesDawn.Presentation.Audio
{
    /// <summary>
    /// Voix des répliques (dialogues de narration, cinématique) : un fichier par réplique et par langue dans Resources/Voices/&lt;fr|en&gt;/.
    /// Sans fichier, rien ne se joue et le sous-titre fait foi. La musique baisse pendant la réplique.
    /// </summary>
    public interface IVoicePlayer
    {
        /// <summary>Un fichier existe-t-il pour cette réplique dans la langue courante ?</summary>
        bool Has(string lineKey);

        /// <summary>Joue la réplique (la précédente s'arrête) ; sans effet si elle n'a pas de fichier.</summary>
        void Play(string lineKey);

        void Stop();
    }
}
