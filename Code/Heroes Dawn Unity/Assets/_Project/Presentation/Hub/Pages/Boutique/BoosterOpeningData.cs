#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Une carte du booster : nom, rareté (couleur et rang 0 à 3), initiale, « nouvelle » ou doublon.</summary>
    public struct BoosterCardData
    {
        public string Name;
        public string RarityName;
        public Color RarityColor;
        /// <summary>0 commune, 1 rare, 2 épique, 3 légendaire : règle l'intensité de l'éclat à la révélation.</summary>
        public int RarityRank;
        public string Glyph;
        public bool IsNew;
    }

    /// <summary>Tout ce qu'affiche l'ouverture d'un booster.</summary>
    public struct BoosterOpeningData
    {
        public string Title;
        public string TapHint;
        public string SkipLabel;
        public string ContinueLabel;
        public string NewLabel;
        public BoosterCardData[] Cards;
    }

    /// <summary>Ouverture animée d'un booster : paquet, éclat, cartes retournées une à une. Annulable : Retour vaut « Continuer ».</summary>
    public interface IBoosterOpeningPopupView : IPopupView
    {
        /// <summary>Joue l'ouverture ; se termine sur « Continuer » (ou Retour).</summary>
        UniTask ShowAsync(BoosterOpeningData data, CancellationToken ct);
    }
}
