#nullable enable
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Suivi des quêtes (popup) : quêtes principales et secondaires ; se termine à la fermeture.</summary>
    public interface IQuestLogPopupView : IPopupView
    {
        /// <summary>Le joueur réclame la récompense d'une quête secondaire (identifiant).</summary>
        event Action<string>? ClaimClicked;

        UniTask ShowAsync(QuestLogData data, CancellationToken ct);

        /// <summary>Met à jour la liste (après une récompense réclamée).</summary>
        void Refresh(QuestLogData data);
    }
}
