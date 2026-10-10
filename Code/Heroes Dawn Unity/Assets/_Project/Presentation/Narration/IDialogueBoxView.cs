#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;

namespace N29.HeroesDawn.Presentation.Narration
{
    /// <summary>Réplique prête à afficher : qui parle (nom, portrait, côté de l'écran) et ce qu'il dit.</summary>
    public readonly struct DialogueBoxLine
    {
        public string Speaker { get; }
        public PortraitData Portrait { get; }
        public string Text { get; }
        /// <summary>Vrai : le locuteur est à gauche (les agents) ; faux : à droite (les autres personnages).</summary>
        public bool Left { get; }

        public DialogueBoxLine(string speaker, PortraitData portrait, string text, bool left)
        {
            Speaker = speaker; Portrait = portrait; Text = text; Left = left;
        }
    }

    /// <summary>Boîte de dialogue de narration : avatar, nom, bulle de bande dessinée, texte qui s'écrit ; un toucher passe à la réplique suivante.</summary>
    public interface IDialogueBoxView : IPopupView
    {
        /// <summary>Joue les répliques dans l'ordre ; rend la main à la dernière (ou dès que le joueur passe le dialogue).</summary>
        UniTask PlayAsync(IReadOnlyList<DialogueBoxLine> lines, CancellationToken ct);
    }
}
