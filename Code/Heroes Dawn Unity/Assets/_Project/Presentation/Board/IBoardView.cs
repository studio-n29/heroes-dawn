#nullable enable
using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.Hub;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Board
{
    /// <summary>Plateau : carte, objectifs, escouade, dé, HUD. Les raccourcis (Espace, B, Entrée) sont relayés en intentions.</summary>
    public interface IBoardView : IView
    {
        event Action? RollClicked;
        event Action? BonusClicked;
        event Action? EndMoveClicked;
        event Action? BackClicked;
        /// <summary>« S'y rendre » sur la carte d'information de la case visée.</summary>
        event Action? MoveConfirmClicked;
        event Action? MoveCancelClicked;

        IWalletView Wallet { get; }
        IBoardMapView Map { get; }

        void SetZoneTitle(string title);
        void ShowObjectives(IReadOnlyList<ObjectiveLineData> objectives);
        void ShowSquad(IReadOnlyList<SquadSlotData> squad);
        void ShowHud(BoardHudData hud);

        /// <summary>Affiche la carte d'information de la case visée ; <c>null</c> la ferme.</summary>
        void ShowMovePreview(MovePreviewData? data);

        /// <summary>Anime le dé (faces aléatoires) puis affiche <paramref name="value"/>.</summary>
        UniTask RollDieAsync(int value, int faces, CancellationToken ct);
        void ResetDie();

        /// <summary>Onomatopée de bande dessinée plein écran (« COMBAT ! ») jouée avant l'action ; rend la main quand elle est finie.</summary>
        UniTask PlayCueAsync(ComicCueData cue, CancellationToken ct);

        /// <summary>Temps de lecture entre deux étapes (annonce puis case, zone sécurisée).</summary>
        UniTask PauseAsync(float seconds, CancellationToken ct);
    }
}
