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

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Carte tirée d'un booster (UI Toolkit) : dos au départ, puis retournement avec un éclat à la couleur de la rareté.</summary>
    public sealed class BoosterCardHud
    {
        readonly VisualElement _root = new VisualElement(), _face = new VisualElement(), _back = new VisualElement(), _art = new VisualElement(), _newBadge = new VisualElement();
        readonly Label _glyph = new Label(), _name = new Label(), _desc = new Label(), _rarity = new Label(), _newText = new Label();
        BoosterCardData _data;

        public VisualElement Root => _root;
        public bool Revealed { get; private set; }
        public int RarityRank => _data.RarityRank;
        public bool IsNew => _data.IsNew;
        public string Name => _data.Name;
        public bool FaceVisible => _face.style.display == DisplayStyle.Flex;

        public BoosterCardHud(BoosterCardData d, string newLabel)
        {
            _data = d;
            _root.AddToClassList("hd-boost__card");
            _face.AddToClassList("hd-boost__face"); _back.AddToClassList("hd-boost__back");
            _art.AddToClassList("hd-boost__art"); _glyph.AddToClassList("hd-boost__glyph");
            _name.AddToClassList("hd-boost__name"); _desc.AddToClassList("hd-boost__desc"); _rarity.AddToClassList("hd-boost__rarity");
            _newBadge.AddToClassList("hd-boost__new"); _newText.AddToClassList("hd-boost__newtext"); _newBadge.Add(_newText);
            _art.Add(_glyph);
            foreach (var e in new VisualElement[] { _art, _name, _desc, _rarity, _newBadge }) _face.Add(e);
            _root.Add(_back); _root.Add(_face);
            foreach (var e in new VisualElement[] { _root, _face, _back, _art, _newBadge, _glyph, _name, _desc, _rarity, _newText }) e.pickingMode = PickingMode.Ignore;
            _glyph.text = d.Glyph;
            _art.style.backgroundImage = d.Art != null ? new StyleBackground(Background.FromSprite(d.Art)) : new StyleBackground(StyleKeyword.None);
            _glyph.style.display = d.Art == null ? DisplayStyle.Flex : DisplayStyle.None;
            _desc.text = d.Description ?? ""; _name.text = d.Name; _rarity.text = d.RarityName;
            _rarity.style.color = Ui.ReadableText(Theme.Surface, d.RarityColor);
            _newText.text = newLabel;
            _face.style.display = DisplayStyle.None; _back.style.display = DisplayStyle.Flex; _newBadge.style.display = DisplayStyle.None;
            SetBorder(Theme.FrostEdge);
        }

        void SetBorder(Color c) => _root.style.borderTopColor = _root.style.borderBottomColor = _root.style.borderLeftColor = _root.style.borderRightColor = c;

        /// <summary>Retourne la carte ; <paramref name="animate"/> faux : tout de suite, sans éclat.</summary>
        public void Reveal(bool animate, VisualElement fxLayer)
        {
            if (Revealed) return;
            Revealed = true;
            if (!animate || !HudAnim.Allowed) { ShowFace(); return; }
            HudAnim.Run(_root, 0.12f, k => HudAnim.SetScale(_root, new Vector2(1f - k, 1f)), () =>
            {
                ShowFace();
                HudAnim.Run(_root, 0.16f, k => HudAnim.SetScale(_root, new Vector2(k, 1f + 0.08f * Mathf.Sin(k * Mathf.PI))),
                            () => { HudAnim.SetScale(_root, 1f); Burst(fxLayer); }, HudAnim.EaseOutBack);
            });
        }

        void ShowFace()
        {
            _back.style.display = DisplayStyle.None; _face.style.display = DisplayStyle.Flex;
            SetBorder(_data.RarityColor);
            _newBadge.style.display = _data.IsNew ? DisplayStyle.Flex : DisplayStyle.None;
        }

        void Burst(VisualElement fxLayer)
        {
            HudAnim.Sparks(fxLayer, _root, _data.RarityColor, 6 + 5 * _data.RarityRank, 110f + 30f * _data.RarityRank, 14f + 2f * _data.RarityRank);
            if (_data.RarityRank >= 2) HudAnim.Punch(_root, 0.1f, 0.3f);
        }

        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void HideCard() => _root.style.display = DisplayStyle.None;
        public void ResetScale() => HudAnim.SetScale(_root, 1f);
    }

    /// <summary>
    /// Ouverture d'un booster (fenêtre UI Toolkit plein écran) : le paquet attend un toucher, s'agite puis éclate en un flash, les cartes sortent
    /// et se retournent une à une (éclat selon la rareté) ; un toucher pendant la révélation retourne tout, « Continuer » ferme.
    /// </summary>
    public sealed class BoosterOpeningPopupView : HdPopup, IBoosterOpeningPopupView
    {
        public enum Phase { Idle, Sealed, Opening, Revealing, Done, Summary }

        readonly List<BoosterCardHud> _cards = new List<BoosterCardHud>();
        UnityEngine.UIElements.Button _tap = null!, _skip = null!, _continue = null!;
        VisualElement _pack = null!, _row = null!, _flash = null!;
        Label _title = null!, _hint = null!;
        UniTaskCompletionSource? _done;
        string _continueLabel = "";
        BoosterOpeningData? _last;

        public override bool Cancelable => true;
        public override void RequestCancel() => _done?.TrySetResult();
        protected override VisualElement DefaultFocus => _tap;

        public Phase Current { get; private set; } = Phase.Idle;
        public IReadOnlyList<BoosterCardHud> Cards => _cards;
        public HdButtonRef TapButton { get; private set; } = null!;
        public HdButtonRef SkipButton { get; private set; } = null!;
        public HdButtonRef ContinueButton { get; private set; } = null!;
        public bool ContinueVisible => _continue != null && _continue.style.display == DisplayStyle.Flex;
        /// <summary>Texte d'invite sous le titre (ouverture, puis résumé) ; tests.</summary>
        public string Hint => _hint != null ? _hint.text : "";
        public string Title => _title != null ? _title.text : "";

        protected override void Bind()
        {
            _tap = Q<UnityEngine.UIElements.Button>("tap"); _skip = Q<UnityEngine.UIElements.Button>("skip"); _continue = Q<UnityEngine.UIElements.Button>("continue");
            _pack = Q<VisualElement>("pack"); _row = Q<VisualElement>("cards"); _flash = Q<VisualElement>("flash");
            _title = Q<Label>("title"); _hint = Q<Label>("hint");
            _flash.pickingMode = PickingMode.Ignore;
            TapButton = new HdButtonRef(_tap, this); SkipButton = new HdButtonRef(_skip, this); ContinueButton = new HdButtonRef(_continue, this);
            _tap.clicked += OnTap;
            _skip.clicked += RevealAll;
            _continue.clicked += () =>
            {
                if (Current == Phase.Done) ShowSummary();
                else if (Current == Phase.Summary) _done?.TrySetResult();
            };
            if (_last.HasValue) Fill(_last.Value);
        }

        void Fill(BoosterOpeningData d)
        {
            _title.text = d.Title; _hint.text = d.TapHint;
            _skip.text = d.SkipLabel; _continue.text = d.ContinueLabel;
            _continueLabel = d.ContinueLabel;
            HudAnim.SetScale(_row, 1f);
            _row.Clear(); _cards.Clear();
            foreach (var c in d.Cards)
            {
                var card = new BoosterCardHud(c, d.NewLabel);
                card.HideCard();
                _row.Add(card.Root); _cards.Add(card);
            }
            _flash.style.opacity = 0f;
            _pack.style.display = DisplayStyle.Flex; HudAnim.SetScale(_pack, 1f);
            _continue.style.display = DisplayStyle.None; _skip.style.display = DisplayStyle.None;
            _hint.style.display = DisplayStyle.Flex;
        }

        public async UniTask ShowAsync(BoosterOpeningData d, CancellationToken ct)
        {
            _last = d;
            await UniTask.WaitUntil(() => _row != null, cancellationToken: ct); // le document se monte au premier affichage
            Fill(d);
            Current = Phase.Sealed;
            HudAnim.PopIn(_pack, 0.3f);
            _done = new UniTaskCompletionSource();
            using (ct.Register(() => _done.TrySetCanceled(ct)))
            {
                try { await _done.Task; }
                finally { Current = Phase.Idle; }
            }
        }

        void OnTap()
        {
            if (Current == Phase.Sealed) StartOpening();
            else if (Current == Phase.Revealing) RevealAll();
        }

        /// <summary>Le paquet s'agite, un flash couvre l'écran, les cartes sortent face cachée.</summary>
        void StartOpening()
        {
            Current = Phase.Opening;
            _hint.style.display = DisplayStyle.None;
            if (!HudAnim.Allowed)
            {
                ShowCards(false);
                Current = Phase.Revealing;
                RevealAll();
                return;
            }
            HudAnim.Shake(_pack, 16f, 0.45f);
            HudAnim.Run(_flash, 0.45f, k => _flash.style.opacity = k * k, () =>
            {
                if (Current != Phase.Opening) return;
                _pack.style.display = DisplayStyle.None;
                ShowCards(true);
                HudAnim.Run(_flash, 0.35f, k => _flash.style.opacity = 1f - k);
                StartReveals();
            });
        }

        void ShowCards(bool animate)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].Show();
                if (animate) { var card = _cards[i]; HudAnim.After(_row, i * 0.1f, () => HudAnim.PopIn(card.Root, 0.28f)); }
            }
            _skip.style.display = DisplayStyle.Flex;
        }

        void StartReveals()
        {
            Current = Phase.Revealing;
            for (int i = 0; i < _cards.Count; i++)
            {
                var card = _cards[i];
                HudAnim.After(_row, 0.6f + i * 0.6f, () =>
                {
                    if (Current != Phase.Revealing) return;
                    card.Reveal(true, Root);
                    CheckDone(0.4f);
                });
            }
        }

        /// <summary>Retourne toutes les cartes tout de suite (toucher pendant la révélation, « Passer »).</summary>
        public void RevealAll()
        {
            if (Current == Phase.Sealed) { StartOpening(); return; }
            if (Current == Phase.Done || Current == Phase.Summary || Current == Phase.Idle) return;
            _pack.style.display = DisplayStyle.None;
            foreach (var c in _cards) { c.Show(); c.ResetScale(); c.Reveal(false, Root); }
            _flash.style.opacity = 0f;
            Current = Phase.Revealing;
            CheckDone(0f);
        }

        void CheckDone(float afterSeconds)
        {
            foreach (var c in _cards) if (!c.Revealed) return;
            if (Current == Phase.Done) return;
            Current = Phase.Done;
            if (afterSeconds <= 0f) EndReveal();
            else HudAnim.After(_row, afterSeconds, EndReveal);
        }

        void EndReveal()
        {
            _skip.style.display = DisplayStyle.None;
            _continue.style.display = DisplayStyle.Flex;
            _continue.text = Loc.T("booster.next");
            HudAnim.PopIn(_continue, 0.22f);
        }

        void ShowSummary()
        {
            Current = Phase.Summary;
            _title.text = Loc.T("booster.summary");
            _hint.style.display = DisplayStyle.Flex;
            _hint.text = Loc.F("booster.received", _cards.Count);
            HudAnim.SetScale(_row, 0.72f);
            _continue.text = _continueLabel;
        }
    }
}
