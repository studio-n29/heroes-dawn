#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;

namespace N29.HeroesDawn.Application.Ports
{
    /// <summary>Chargement des scènes de jeu (adaptateur Unity : UnitySceneLoader).</summary>
    public interface ISceneLoader
    {
        /// <summary>Charge une scène en additif.</summary>
        UniTask LoadAsync(string scene, CancellationToken ct);
        UniTask UnloadAsync(string scene, CancellationToken ct);
        /// <summary>Affiche ou masque une scène chargée (masquée = objets désactivés, donc en pause).</summary>
        void SetVisible(string scene, bool visible);
    }
}
