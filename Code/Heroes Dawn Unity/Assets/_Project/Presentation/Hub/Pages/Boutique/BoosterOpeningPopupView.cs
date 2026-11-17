#nullable enable
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using N29.HeroesDawn.Presentation.Audio;
using N29.HeroesDawn.Presentation.Combat.Hud;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Feedback;
using N29.HeroesDawn.Presentation.UiToolkit;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Hub
{
    /// <summary>Carte tirée d'un booster (UI Toolkit) : dos au départ, puis retournement avec une mise en scène selon sa rareté.</summary>
    public sealed class BoosterCardHud
    {
        readonly VisualElement _root = new VisualElement(), _face = new VisualElement(), _back = new VisualElement(), _art = new VisualElement(), _badge = new VisualElement(), _emblem = new VisualElement();
        readonly Label _glyph = new Label(), _name = new Label(), _desc = new Label(), _rarity = new Label(), _badgeText = new Label();
        BoosterCardData _data;

        public VisualElement Root => _root;
        public bool Revealed { get; private set; }
        public int RarityRank => _data.RarityRank;
        public bool IsNew => _data.IsNew;
        public string Name => _data.Name;
        public bool FaceVisible => _face.style.display == DisplayStyle.Flex;
        /// <summary>Texte du badge (« NOUVEAU » ou « DOUBLON ») une fois la carte retournée ; tests.</summary>
        public string BadgeText => _badgeText.text;
        public bool BadgeVisible => _badge.style.display == DisplayStyle.Flex;

        public BoosterCardHud(BoosterCardData d, string newLabel, string duplicateLabel)
        {
            _data = d;
            _root.AddToClassList("hd-boost__card");
            _face.AddToClassList("hd-boost__face"); _back.AddToClassList("hd-boost__back"); _emblem.AddToClassList("hd-boost__emblem");
            _back.Add(_emblem);
            _art.AddToClassList("hd-boost__art"); _glyph.AddToClassList("hd-boost__glyph");
            _name.AddToClassList("hd-boost__name"); _desc.AddToClassList("hd-boost__desc"); _rarity.AddToClassList("hd-boost__rarity");
            _badge.AddToClassList("hd-boost__new"); _badgeText.AddToClassList("hd-boost__newtext"); _badge.Add(_badgeText);
            if (!d.IsNew) _badge.AddToClassList("hd-boost__new--dup");
            _art.Add(_glyph);
            foreach (var e in new VisualElement[] { _art, _name, _desc, _rarity, _badge }) _face.Add(e);
            _root.Add(_back); _root.Add(_face);
            foreach (var e in new VisualElement[] { _root, _face, _back, _emblem, _art, _badge, _glyph, _name, _desc, _rarity, _badgeText }) e.pickingMode = PickingMode.Ignore;
            _glyph.text = d.Glyph;
            _art.style.backgroundImage = d.Art != null ? new StyleBackground(Background.FromSprite(d.Art)) : new StyleBackground(StyleKeyword.None);
            _glyph.style.display = d.Art == null ? DisplayStyle.Flex : DisplayStyle.None;
            _desc.text = d.Description ?? ""; _name.text = d.Name; _rarity.text = d.RarityName;
            _rarity.style.color = Ui.ReadableText(Theme.Surface, d.RarityColor);
            _badgeText.text = d.IsNew ? newLabel : duplicateLabel;
            _face.style.display = DisplayStyle.None; _back.style.display = DisplayStyle.Flex; _badge.style.display = DisplayStyle.None;
            SetBorder(Theme.FrostEdge);
        }

        void SetBorder(Color c) => _root.style.borderTopColor = _root.style.borderBottomColor = _root.style.borderLeftColor = _root.style.borderRightColor = c;

        /// <summary>Retourne la carte ; <paramref name="animate"/> faux : tout de suite, sans éclat. <paramref name="onBurst"/> : au moment de l'éclat.</summary>
        public void Reveal(bool animate, VisualElement fxLayer, System.Action? onBurst = null)
        {
            if (Revealed) return;
            Revealed = true;
            if (!animate || !HudAnim.Allowed) { ShowFace(); return; }
            var style = RevealStyles.For(_data.RarityRank);
            void Flip()
            {
                HudAnim.Run(_root, 0.12f, k => HudAnim.SetScale(_root, new Vector2(1f - k, 1f)), () =>
                {
                    ShowFace();
                    HudAnim.Run(_root, 0.16f, k => HudAnim.SetScale(_root, new Vector2(k, 1f + 0.08f * Mathf.Sin(k * Mathf.PI))),
                                () => { HudAnim.SetScale(_root, 1f); Burst(fxLayer, style); onBurst?.Invoke(); }, HudAnim.EaseOutBack);
                });
            }
            // Suspense : la carte vibre avant de se retourner (légendaire).
            if (style.PauseSeconds > 0f)
            {
                HudAnim.Shake(_root, 6f, style.PauseSeconds);
                HudAnim.After(_root, style.PauseSeconds, Flip);
            }
            else Flip();
        }

        void ShowFace()
        {
            _back.style.display = DisplayStyle.None; _face.style.display = DisplayStyle.Flex;
            SetBorder(_data.RarityColor);
            _badge.style.display = DisplayStyle.Flex;
            _root.style.unityBackgroundImageTintColor = Color.white;
        }

        void Burst(VisualElement fxLayer, RevealStyle style)
        {
            HudAnim.Sparks(fxLayer, _root, _data.RarityColor, style.Sparks, 110f + 30f * _data.RarityRank, 14f + 2f * _data.RarityRank);
            if (style.Punch) HudAnim.Punch(_root, 0.1f, 0.3f);
        }

        public void Mark(bool best) { if (best) _root.AddToClassList("hd-boost__card--best"); else _root.RemoveFromClassList("hd-boost__card--best"); }
        public void Show() => _root.style.display = DisplayStyle.Flex;
        public void HideCard() => _root.style.display = DisplayStyle.None;
        public void ResetScale() => HudAnim.SetScale(_root, 1f);
    }

    /// <summary>
    /// Ouverture d'un booster (fenêtre UI Toolkit plein écran) : le paquet fermé attend un toucher, s'agite, se déchire et éclate en lumière ;
    /// les cartes sortent face cachée et le joueur touche pour les retourner une à une (mise en scène selon la rareté) ; un récapitulatif
    /// montre toutes les cartes, les nouvelles et les doublons. La logique est dans <see cref="BoosterRevealSequence"/>.
    /// </summary>
    public sealed class BoosterOpeningPopupView : HdPopup, IBoosterOpeningPopupView
    {
        readonly List<BoosterCardHud> _cards = new List<BoosterCardHud>();
        UnityEngine.UIElements.Button _tap = null!, _skip = null!, _continue = null!, _again = null!;
        VisualElement _pack = null!, _packArt = null!, _row = null!, _flash = null!, _rays = null!, _rays2 = null!;
        Label _title = null!, _hint = null!, _best = null!;
        UniTaskCompletionSource<bool>? _done;
        BoosterRevealSequence _seq = new BoosterRevealSequence(System.Array.Empty<RevealCard>());
        BoosterOpeningData? _last;
        float _lockUntil;
        bool _closed;

        public override bool Cancelable => true;
        public override void RequestCancel() => Finish(false);
        protected override VisualElement DefaultFocus => _tap;

        public RevealPhase Current => _seq.Phase;
        public IReadOnlyList<BoosterCardHud> Cards => _cards;
        public HdButtonRef TapButton { get; private set; } = null!;
        public HdButtonRef SkipButton { get; private set; } = null!;
        public HdButtonRef ContinueButton { get; private set; } = null!;
        public HdButtonRef AgainButton { get; private set; } = null!;
        public bool ContinueVisible => _continue != null && _continue.style.display == DisplayStyle.Flex;
        public bool AgainVisible => _again != null && _again.style.display == DisplayStyle.Flex;
        /// <summary>Texte d'invite sous le titre (ouverture, retournement, récapitulatif) ; tests.</summary>
        public string Hint => _hint != null ? _hint.text : "";
        public string Title => _title != null ? _title.text : "";
        public string BestLine => _best != null ? _best.text : "";

        static Sprite? PackArt(string state) => TacticalArt.Load("Missing20261116/Boosters/booster-" + state);

        protected override void Bind()
        {
            _tap = Q<UnityEngine.UIElements.Button>("tap"); _skip = Q<UnityEngine.UIElements.Button>("skip");
            _continue = Q<UnityEngine.UIElements.Button>("continue"); _again = Q<UnityEngine.UIElements.Button>("again");
            _pack = Q<VisualElement>("pack"); _packArt = Q<VisualElement>("packArt"); _row = Q<VisualElement>("cards"); _flash = Q<VisualElement>("flash");
            _rays = Q<VisualElement>("rays"); _rays2 = Q<VisualElement>("rays2");
            _title = Q<Label>("title"); _hint = Q<Label>("hint"); _best = Q<Label>("best");
            _flash.pickingMode = PickingMode.Ignore; _rays.pickingMode = PickingMode.Ignore; _rays2.pickingMode = PickingMode.Ignore;
            TapButton = new HdButtonRef(_tap, this); SkipButton = new HdButtonRef(_skip, this); ContinueButton = new HdButtonRef(_continue, this); AgainButton = new HdButtonRef(_again, this);
            _tap.clicked += OnTap;
            _skip.clicked += Skip;
            _continue.clicked += OnContinue;
            _again.clicked += () => { if (_seq.Phase == RevealPhase.Recap) Finish(true); };
            if (_last.HasValue) Fill(_last.Value);
        }

        void Sfx(SfxId id) => _last?.PlaySfx?.Invoke(id);

        void Fill(BoosterOpeningData d)
        {
            var list = new RevealCard[d.Cards.Length];
            for (int i = 0; i < list.Length; i++) list[i] = new RevealCard(d.Cards[i].RarityRank, d.Cards[i].IsNew);
            _seq = new BoosterRevealSequence(list);
            _closed = false; _lockUntil = 0f;
            _title.text = d.Title; _hint.text = d.TapHint; _best.text = ""; _best.style.display = DisplayStyle.None;
            _skip.text = d.SkipLabel; _continue.text = d.ContinueLabel;
            _again.text = d.OpenAnotherLabel;
            HudAnim.SetScale(_row, 1f);
            _row.Clear(); _cards.Clear();
            foreach (var c in d.Cards)
            {
                var card = new BoosterCardHud(c, d.NewLabel, d.DuplicateLabel);
                card.HideCard();
                _row.Add(card.Root); _cards.Add(card);
            }
            _flash.style.opacity = 0f;
            SetRays(false, Color.white);
            var closed = PackArt("closed");
            _pack.EnableInClassList("hd-boost__pack--art", closed != null);
            _packArt.style.backgroundImage = closed != null ? new StyleBackground(Background.FromSprite(closed)) : new StyleBackground(StyleKeyword.None);
            _pack.style.display = DisplayStyle.Flex; HudAnim.SetScale(_pack, 1f); HudAnim.SetTranslate(_pack, 0f, 0f);
            _continue.style.display = DisplayStyle.None; _skip.style.display = DisplayStyle.Flex; _again.style.display = DisplayStyle.None;
            _hint.style.display = DisplayStyle.Flex;
        }

        public async UniTask<bool> ShowAsync(BoosterOpeningData d, CancellationToken ct)
        {
            _last = d;
            await UniTask.WaitUntil(() => _row != null, cancellationToken: ct); // le document se monte au premier affichage
            Fill(d);
            HudAnim.PopIn(_pack, 0.3f);
            _done = new UniTaskCompletionSource<bool>();
            using (ct.Register(() => _done.TrySetCanceled(ct)))
            {
                return await _done.Task;
            }
        }

        void Finish(bool openAnother)
        {
            if (_closed) return;
            _closed = true;
            _seq.Close(openAnother);
            _done?.TrySetResult(openAnother);
        }

        bool Locked => Time.unscaledTime < _lockUntil;
        void Lock(float seconds) => _lockUntil = Time.unscaledTime + seconds;

        void OnTap()
        {
            if (Locked) return;
            var a = _seq.Tap();
            switch (a.Kind)
            {
                case SeqActionKind.StartOpening: StartOpening(); break;
                case SeqActionKind.Reveal: RevealCard(a.Index); break;
                case SeqActionKind.ToRecap: ShowRecap(); break;
            }
        }

        void OnContinue()
        {
            if (_seq.Phase == RevealPhase.AllRevealed) { _seq.Tap(); ShowRecap(); }
            else if (_seq.Phase == RevealPhase.Recap) Finish(false);
        }

        /// <summary>Passer : retourne tout et va au récapitulatif.</summary>
        public void Skip()
        {
            if (_seq.Phase == RevealPhase.Recap || _seq.Phase == RevealPhase.Closed) return;
            _seq.Skip();
            _pack.style.display = DisplayStyle.None;
            SetRays(false, Color.white);
            _flash.style.opacity = 0f;
            foreach (var c in _cards) { c.Show(); c.ResetScale(); c.Reveal(false, Root); }
            _lockUntil = 0f;
            ShowRecap();
        }

        void SetRays(bool on, Color tint)
        {
            var sprite = PackArt("rays");
            foreach (var r in new[] { _rays, _rays2 })
            {
                r.style.display = on && sprite != null ? DisplayStyle.Flex : DisplayStyle.None;
                r.style.backgroundImage = sprite != null ? new StyleBackground(Background.FromSprite(sprite)) : new StyleBackground(StyleKeyword.None);
                r.style.unityBackgroundImageTintColor = tint;
            }
        }

        /// <summary>Éclat de rayons derrière une carte rare ou mieux : deux demi-éventails posés dans la rangée, sous les cartes, teintés de la rareté.</summary>
        void CardRays(BoosterCardHud card, Color tint, float scale)
        {
            var sprite = PackArt("rays");
            if (sprite == null || !HudAnim.Allowed) return;
            var c = card.Root.layout.center;
            float w = 1200f * scale, h = 600f * scale;
            VisualElement Make(float top, bool flip)
            {
                var r = new VisualElement { pickingMode = PickingMode.Ignore };
                r.AddToClassList("hd-boost__cardrays");
                r.style.width = w; r.style.height = h; r.style.left = c.x - w * 0.5f; r.style.top = top;
                r.style.backgroundImage = new StyleBackground(Background.FromSprite(sprite));
                r.style.unityBackgroundImageTintColor = tint;
                if (flip) HudAnim.SetRotate(r, 180f);
                _row.Insert(0, r);
                return r;
            }
            var up = Make(c.y - h, false); var down = Make(c.y, true);
            HudAnim.Run(up, 0.7f, k => { up.style.opacity = 0.5f * (1f - k); down.style.opacity = 0.5f * (1f - k); },
                        () => { up.RemoveFromHierarchy(); down.RemoveFromHierarchy(); });
        }

        void FadeRays(float seconds, float peak)
        {
            if (!HudAnim.Allowed) { SetRays(false, Color.white); return; }
            HudAnim.Run(_rays, seconds, k => { float o = peak * (1f - k); _rays.style.opacity = o; _rays2.style.opacity = o; },
                        () => { _rays.style.display = DisplayStyle.None; _rays2.style.display = DisplayStyle.None; });
        }

        /// <summary>Le paquet s'agite, se déchire (flash, son), éclate en lumière ; les cartes sortent face cachée.</summary>
        void StartOpening()
        {
            _hint.style.display = DisplayStyle.None;
            if (!HudAnim.Allowed)
            {
                _pack.style.display = DisplayStyle.None;
                ShowCards(false);
                _seq.OpeningFinished();
                OnRevealingStarted();
                return;
            }
            var torn = PackArt("torn"); var burst = PackArt("reveal");
            HudAnim.Shake(_pack, 16f, 0.5f);
            HudAnim.Run(_flash, 0.5f, k => _flash.style.opacity = 0.35f * k, () =>
            {
                if (_seq.Phase != RevealPhase.Opening) return;
                Sfx(SfxId.BoosterOpen);
                if (torn != null) _packArt.style.backgroundImage = new StyleBackground(Background.FromSprite(torn));
                HudAnim.Punch(_pack, 0.08f, 0.25f);
                HudAnim.After(_pack, 0.3f, () =>
                {
                    if (_seq.Phase != RevealPhase.Opening) return;
                    if (burst != null) _packArt.style.backgroundImage = new StyleBackground(Background.FromSprite(burst));
                    SetRays(true, new Color(1f, 0.85f, 0.45f));
                    HudAnim.Run(_flash, 0.3f, k => _flash.style.opacity = 0.35f + 0.65f * k, () =>
                    {
                        if (_seq.Phase != RevealPhase.Opening) return;
                        _pack.style.display = DisplayStyle.None;
                        ShowCards(true);
                        FadeRays(0.6f, 0.6f);
                        HudAnim.Run(_flash, 0.35f, k => _flash.style.opacity = 1f - k);
                        HudAnim.After(_row, 0.5f, () =>
                        {
                            if (_seq.Phase != RevealPhase.Opening) return;
                            _seq.OpeningFinished();
                            OnRevealingStarted();
                        });
                    });
                });
            });
        }

        void OnRevealingStarted()
        {
            _hint.style.display = DisplayStyle.Flex;
            _hint.text = _last?.FlipHint ?? "";
            if (_seq.Phase == RevealPhase.AllRevealed) EndReveal();
        }

        void ShowCards(bool animate)
        {
            for (int i = 0; i < _cards.Count; i++)
            {
                _cards[i].Show();
                if (animate) { var card = _cards[i]; HudAnim.After(_row, i * 0.1f, () => HudAnim.PopIn(card.Root, 0.28f)); }
            }
        }

        void RevealCard(int index)
        {
            var card = _cards[index];
            var style = RevealStyles.For(card.RarityRank);
            var sfx = card.RarityRank >= 3 ? SfxId.CardFlipLegendary : card.RarityRank == 2 ? SfxId.CardFlipEpic : card.RarityRank == 1 ? SfxId.CardFlipRare : SfxId.CardFlip;
            Lock((HudAnim.Allowed ? style.PauseSeconds : 0f) + (HudAnim.Allowed ? 0.3f : 0f));
            if (!HudAnim.Allowed) { Sfx(sfx); card.Reveal(false, Root); AfterReveal(); return; }
            card.Reveal(true, Root, () =>
            {
                Sfx(sfx);
                if (style.Rays) CardRays(card, card.RarityRank >= 3 ? new Color(1f, 0.85f, 0.45f) : card.Root.resolvedStyle.borderTopColor, 0.6f + 0.15f * card.RarityRank);
                if (style.ScreenFlash) { _flash.style.unityBackgroundImageTintColor = Color.white; HudAnim.Run(_flash, 0.4f, k => _flash.style.opacity = 0.6f * (1f - k)); }
            });
            HudAnim.After(_row, (style.PauseSeconds) + 0.3f, AfterReveal);
        }

        void AfterReveal()
        {
            if (_seq.Phase == RevealPhase.AllRevealed) EndReveal();
        }

        void EndReveal()
        {
            if (_closed) return;
            _skip.style.display = DisplayStyle.None;
            _continue.style.display = DisplayStyle.Flex;
            _continue.text = Loc.T("booster.next");
            _hint.style.display = DisplayStyle.None;
            HudAnim.PopIn(_continue, 0.22f);
        }

        void ShowRecap()
        {
            var d = _last.GetValueOrDefault();
            _title.text = d.RecapTitle ?? Loc.T("booster.summary");
            _hint.style.display = DisplayStyle.Flex;
            _hint.text = (d.RecapLine != null ? d.RecapLine(_seq.NewCount, _seq.DuplicateCount) + " · " : "") + (d.ReceivedLine != null ? d.ReceivedLine(_seq.Count) : "");
            _skip.style.display = DisplayStyle.None;
            HudAnim.SetScale(_row, 0.72f);
            int best = _seq.BestIndex;
            for (int i = 0; i < _cards.Count; i++) _cards[i].Mark(i == best && _cards.Count > 1);
            if (best >= 0 && d.RecapBestLabel != null) { _best.text = d.RecapBestLabel + " : " + _cards[best].Name; _best.style.display = DisplayStyle.Flex; }
            _continue.text = d.ContinueLabel;
            _continue.style.display = DisplayStyle.Flex;
            bool more = d.RemainingBoosters > 0 && !string.IsNullOrEmpty(d.OpenAnotherLabel);
            _again.style.display = more ? DisplayStyle.Flex : DisplayStyle.None;
            if (more) { _again.text = d.OpenAnotherLabel + " (" + d.RemainingBoosters + ")"; HudAnim.PopIn(_again, 0.22f); }
            Sfx(SfxId.ToastReward);
        }
    }
}
