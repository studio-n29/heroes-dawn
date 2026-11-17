#nullable enable
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Mvp;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Ce que montre la fenêtre d'ouverture d'un chapitre : intitulé, texte, illustration éventuelle, libellé du bouton.</summary>
    public readonly struct ChapterPopupData
    {
        public readonly string Heading;
        public readonly string Text;
        public readonly Sprite? Art;
        public readonly string Continue;
        /// <summary>Illustration 16:9 d'un souvenir (fenêtre LIRE), placée au-dessus du texte ; nulle : aucune. Remplace l'image large du chapitre.</summary>
        public readonly Sprite? MemoryArt;
        public ChapterPopupData(string heading, string text, Sprite? art, string @continue, Sprite? memoryArt = null)
        { Heading = heading; Text = text; Art = art; Continue = @continue; MemoryArt = memoryArt; }
    }

    /// <summary>Fenêtre d'ouverture d'un chapitre : se termine quand le joueur continue (ou revient en arrière).</summary>
    public interface IChapterPopupView : IPopupView
    {
        UniTask ShowAsync(ChapterPopupData data, CancellationToken ct);
    }

    /// <summary>Fenêtre UI Toolkit : image large (masquée sans illustration), intitulé, texte, bouton Continuer. Retour = Continuer.</summary>
    public sealed class ChapterPopupView : HdPopup, IChapterPopupView
    {
        Label _heading = null!, _text = null!;
        VisualElement _art = null!, _memoryArt = null!;
        UnityEngine.UIElements.Button _continue = null!;
        UniTaskCompletionSource? _done;
        ChapterPopupData? _last;

        public override bool Cancelable => true;
        public HdButtonRef ContinueButton { get; private set; } = null!;
        protected override VisualElement DefaultFocus => _continue;

        protected override void Bind()
        {
            _heading = Q<Label>("heading"); _text = Q<Label>("text"); _art = Q<VisualElement>("art");
            _memoryArt = Q<VisualElement>("memoryArt");
            _continue = Q<UnityEngine.UIElements.Button>("continue");
            ContinueButton = new HdButtonRef(_continue, this);
            _continue.clicked += () => _done?.TrySetResult();
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill(ChapterPopupData d)
        {
            _heading.text = d.Heading; _text.text = d.Text; _continue.text = d.Continue;
            _art.style.display = d.Art != null && d.MemoryArt == null ? DisplayStyle.Flex : DisplayStyle.None; // sans illustration, le texte remonte
            HdPopupRows.SetArt(_art, d.Art);
            _memoryArt.style.display = d.MemoryArt != null ? DisplayStyle.Flex : DisplayStyle.None;
            HdPopupRows.SetArt(_memoryArt, d.MemoryArt);
        }

        public UniTask ShowAsync(ChapterPopupData data, CancellationToken ct)
        {
            _last = data;
            if (_heading != null) Fill(data);
            _done = new UniTaskCompletionSource();
            ct.Register(() => _done.TrySetCanceled());
            return _done.Task;
        }

        public override void RequestCancel() => _done?.TrySetResult();
    }
}
