#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Audio;
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
        /// <summary>0 commune, 1 rare, 2 épique, 3 légendaire : règle la mise en scène de la révélation (<see cref="RevealStyles"/>).</summary>
        public int RarityRank;
        public string Glyph;
        /// <summary>Illustration de la carte ; nulle : la pastille d'initiale la remplace.</summary>
        public Sprite? Art;
        /// <summary>Effets de la carte (dégâts, soins… en montants bruts).</summary>
        public string Description;
        /// <summary>Première exemplaire de la collection ; sinon c'est un doublon.</summary>
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
        public string DuplicateLabel;
        /// <summary>Invite pendant la révélation (« Touche pour retourner la carte »).</summary>
        public string FlipHint;
        public string RecapTitle;
        public string RecapBestLabel;
        /// <summary>Libellé du bouton « Ouvrir un autre booster » (vide : pas de bouton).</summary>
        public string OpenAnotherLabel;
        /// <summary>Boosters restants dans la réserve : le bouton « Ouvrir un autre » n'apparaît que s'il en reste.</summary>
        public int RemainingBoosters;
        /// <summary>Formate « X nouvelle(s) · Y doublon(s) » à partir des deux nombres.</summary>
        public Func<int, int, string> RecapLine;
        /// <summary>Formate « n cartes ajoutées au Grimoire ».</summary>
        public Func<int, string> ReceivedLine;
        public BoosterCardData[] Cards;
        /// <summary>Joue un effet sonore (nul : silence). Fourni par l'appelant.</summary>
        public Action<SfxId>? PlaySfx;
    }

    /// <summary>Ouverture animée d'un booster : paquet, éclat, cartes retournées une à une au toucher, récapitulatif. Annulable : Retour vaut « Continuer ».</summary>
    public interface IBoosterOpeningPopupView : IPopupView
    {
        /// <summary>Joue l'ouverture ; se termine sur « Continuer » (ou Retour). Vrai : le joueur veut ouvrir un autre booster.</summary>
        UniTask<bool> ShowAsync(BoosterOpeningData data, CancellationToken ct);
    }
}
