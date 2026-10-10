#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Domain.Activities;
using N29.HeroesDawn.Domain.Activities.Games;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.Popups;
using TMPro;
using UnityEngine;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>La potion : la partie, le livre d'indices, les noms et couleurs des ingrédients, les textes.</summary>
    public sealed class PotionRequest
    {
        public string Title { get; set; } = "";
        public string Intro { get; set; } = "";
        public string Book { get; set; } = "";
        public PotionGame Game { get; set; } = null!;
        public IReadOnlyList<string> IngredientNames { get; set; } = Array.Empty<string>();
        public IReadOnlyList<Color> IngredientColors { get; set; } = Array.Empty<Color>();
        public ILocalization Loc { get; set; } = null!;
    }

    /// <summary>Le dialogue : la partie, le texte de chaque réplique et réponse (par clé), les textes.</summary>
    public sealed class DialogueRequest
    {
        public string Title { get; set; } = "";
        public DialogueGame Game { get; set; } = null!;
        public Func<string, string> Text { get; set; } = k => k;
        public ILocalization Loc { get; set; } = null!;
    }

    public interface IPotionPopupView : IPopupView
    {
        UniTask<ActivityResult> PlayAsync(PotionRequest request, CancellationToken ct);
    }

    public interface IDialoguePopupView : IPopupView
    {
        UniTask<ActivityResult> PlayAsync(DialogueRequest request, CancellationToken ct);
    }
}
