#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Narration
{
    /// <summary>
    /// Boîte de dialogue de narration (fenêtre UI Toolkit) : avatar du locuteur, plaque de nom, bulle de bande dessinée, texte qui s'écrit.
    /// Un toucher achève la réplique, puis passe à la suivante ; « Passer » ferme tout le dialogue.
    /// </summary>
    public sealed class DialogueBoxView : HdPopup, IDialogueBoxView
    {
        const float CharsPerSecond = 55f;
        /// <summary>Pause avant la réplique suivante en mode automatique : fixe, plus un peu par caractère lu.</summary>
        const float AutoBaseSeconds = 1.2f, AutoPerChar = 0.04f;

        /// <summary>Avance automatique choisie par le joueur : elle reste d'un dialogue à l'autre pendant la session.</summary>
        public static bool AutoAdvance { get; set; }

        UnityEngine.UIElements.Button _tap = null!, _skip = null!, _next = null!, _auto = null!;
        VisualElement _stage = null!, _avatarBox = null!, _avatar = null!, _bubble = null!;
        Label _initial = null!, _speaker = null!, _body = null!, _hint = null!, _progress = null!;
        UniTaskCompletionSource? _advance;
        bool _skipped, _finishTyping;
        string _body0 = "";

        public override bool Cancelable => false;
        public override void RequestCancel() { }
        protected override VisualElement DefaultFocus => _tap;

        /// <summary>Réplique affichée (tests).</summary>
        public string Body => _body != null ? _body.text : "";
        public string Speaker => _speaker != null ? _speaker.text : "";
        public bool Typing { get; private set; }
        public HdButtonRef TapButton { get; private set; } = null!;
        public HdButtonRef SkipButton { get; private set; } = null!;
        public HdButtonRef NextButton { get; private set; } = null!;
        public HdButtonRef AutoButton { get; private set; } = null!;
        /// <summary>« 2 / 5 » : la réplique affichée sur le total (tests).</summary>
        public string Progress => _progress != null ? _progress.text : "";

        public event System.Action<DialogueBoxLine>? LineShown;

        protected override void Bind()
        {
            _tap = Q<UnityEngine.UIElements.Button>("tap"); _skip = Q<UnityEngine.UIElements.Button>("skip");
            _next = Q<UnityEngine.UIElements.Button>("next"); _auto = Q<UnityEngine.UIElements.Button>("auto");
            _stage = Q<VisualElement>("stage"); _avatarBox = Q<VisualElement>("avatarBox"); _avatar = Q<VisualElement>("avatar"); _bubble = Q<VisualElement>("bubble");
            _initial = Q<Label>("initial"); _speaker = Q<Label>("speaker"); _body = Q<Label>("body"); _hint = Q<Label>("hint"); _progress = Q<Label>("progress");
            TapButton = new HdButtonRef(_tap, this); SkipButton = new HdButtonRef(_skip, this);
            NextButton = new HdButtonRef(_next, this); AutoButton = new HdButtonRef(_auto, this);
            _skip.text = Loc.T("narration.skip"); _next.text = Loc.T("narration.next_btn"); _auto.text = Loc.T("narration.auto");
            _hint.text = Loc.T("narration.next");
            _tap.clicked += Advance;
            _next.clicked += Advance;
            _skip.clicked += () => { _skipped = true; _advance?.TrySetResult(); };
            _auto.clicked += () => { AutoAdvance = !AutoAdvance; RefreshAuto(); };
            RefreshAuto();
        }

        void Advance() { if (Typing) _finishTyping = true; else _advance?.TrySetResult(); }

        void RefreshAuto()
        {
            _auto.EnableInClassList("hd-btn--primary", AutoAdvance);
            _auto.EnableInClassList("hd-btn--secondary", !AutoAdvance);
        }

        public async UniTask PlayAsync(IReadOnlyList<DialogueBoxLine> lines, CancellationToken ct)
        {
            await UniTask.WaitUntil(() => _body != null, cancellationToken: ct); // le document se monte au premier affichage
            // Un « Passer » reçu avant le début de la lecture compte : l indicateur n est remis à zéro qu à la fin.
            int index = 0;
            foreach (var line in lines)
            {
                if (_skipped) break;
                index++;
                _progress.text = index + " / " + lines.Count;
                Show(line);
                LineShown?.Invoke(line);
                HudAnim.PopIn(_bubble, 0.2f);
                HudAnim.PopIn(_avatarBox, 0.3f);
                _advance = new UniTaskCompletionSource();
                _finishTyping = false;
                _hint.style.display = DisplayStyle.None;
                string text = line.Text;
                _body0 = text;
                _body.text = "";
                Typing = !MotionSettings.Reduced;
                float shown = 0f;
                while (Typing && !_finishTyping && !_skipped && _body.text.Length < text.Length)
                {
                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                    shown += CharsPerSecond * Time.unscaledDeltaTime;
                    _body.text = text.Substring(0, Mathf.Min(text.Length, Mathf.FloorToInt(shown)));
                }
                Typing = false;
                _body.text = text;
                _hint.style.display = DisplayStyle.Flex;
                if (!_skipped) await WaitForAdvanceAsync(_advance, text.Length, ct);
            }
            _advance = null;
            _skipped = false;
        }

        /// <summary>Attend le toucher du joueur ; en mode automatique, la réplique suivante vient d'elle-même après un temps de lecture.</summary>
        static async UniTask WaitForAdvanceAsync(UniTaskCompletionSource advance, int chars, CancellationToken ct)
        {
            float waited = 0f, limit = AutoBaseSeconds + AutoPerChar * chars;
            while (advance.Task.Status == UniTaskStatus.Pending)
            {
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
                if (!AutoAdvance) { waited = 0f; continue; }
                waited += Time.unscaledDeltaTime;
                if (waited >= limit) return;
            }
        }

        void Show(DialogueBoxLine line)
        {
            HdPortrait.Fill(_avatar, _initial, line.Portrait);
            // Un agent détouré passe au premier plan, sans cadre ; les autres personnages gardent leur illustration encadrée.
            var art = line.Portrait.Art;
            var cutout = art != null ? AgentArt.Cutout(art.name) : null;
            // Un personnage non jouable (Homme de main, Passeur) a une illustration générique : sa pose détourée se retrouve par l'identifiant de l'orateur.
            if (cutout == null && !string.IsNullOrEmpty(line.CutoutId)) cutout = AgentArt.Cutout(line.CutoutId);
            if (cutout != null)
            {
                _avatar.style.backgroundImage = new StyleBackground(Background.FromSprite(cutout));
                _avatar.style.backgroundColor = Color.clear;
            }
            _avatar.EnableInClassList("hd-dlg__avatar--cutout", cutout != null);
            _speaker.text = line.Speaker;
            // Plaque de nom ivoire, soulignée de la couleur de la famille.
            _speaker.style.borderBottomColor = Theme.FamilyColor(line.Portrait.Family);
            // L'avatar est du côté du locuteur ; la bulle occupe le reste.
            _stage.style.flexDirection = line.Left ? FlexDirection.Row : FlexDirection.RowReverse;
            _stage.EnableInClassList("hd-dlg__stage--right", !line.Left);
        }
    }
}
