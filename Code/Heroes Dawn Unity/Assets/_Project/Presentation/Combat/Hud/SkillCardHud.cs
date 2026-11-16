#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat.Hud
{
    /// <summary>
    /// Carte de compétence de l'éventail (UI Toolkit) : numéro en losange, illustration pleine carte ou initiale, nom en bas, coût en énergie, anneau quand elle est lançable.
    /// Appui maintenu : description ; glisser : la carte suit le doigt puis, au lâcher, la vue décide de la cible et la carte revient à sa place.
    /// </summary>
    public sealed class SkillCardHud
    {
        const float DragThreshold = 14f;
        readonly VisualElement _root = new VisualElement(), _ring = new VisualElement(), _frame = new VisualElement(), _art = new VisualElement(), _costBadge = new VisualElement(), _slotBadge = new VisualElement(), _shade = new VisualElement(), _plus = new VisualElement();
        readonly Label _glyph = new Label(), _label = new Label(), _cost = new Label(), _slot = new Label();
        string? _artKey;
        Sprite? _cachedArt;
        bool _down, _dragging;
        int _pointer = -1;
        Vector2 _startPanel;
        float _homeRotate;

        public VisualElement Root => _root;
        public SkillSlotData Data { get; private set; }
        public bool CanDrag { get; private set; }
        public bool IsDragging => _dragging;
        /// <summary>Appui maintenu (true) ou relâché (false) sur la carte : affiche sa description d'effet.</summary>
        public event Action<SkillCardHud, bool>? Holding;
        public event Action<SkillCardHud>? DragBegan;
        /// <summary>Carte lâchée à cette position de l'écran (y vers le haut).</summary>
        public event Action<SkillCardHud, Vector2>? DragEnded;

        /// <summary>Décalage vertical de la carte par rapport à sa place, vers le haut positif (animation d'ouverture ; tests).</summary>
        public float OffsetY => -_fanY;
        float _fanY;
        /// <summary>Anime l a place de la carte : décalage vertical (y vers le bas) par rapport au repos.</summary>
        public void SetFanOffset(float y) { _fanY = y; HudAnim.SetTranslate(_root, 0f, y); }

        public SkillCardHud()
        {
            _root.AddToClassList("hd-skill");
            _ring.AddToClassList("hd-skill__ring"); _ring.pickingMode = PickingMode.Ignore;
            _frame.AddToClassList("hd-skill__frame"); _frame.pickingMode = PickingMode.Ignore;
            _art.AddToClassList("hd-skill__art"); _art.pickingMode = PickingMode.Ignore;
            _shade.AddToClassList("hd-skill__shade"); _plus.AddToClassList("hd-skill__plus");
            _glyph.AddToClassList("hd-skill__glyph"); _label.AddToClassList("hd-skill__label");
            _costBadge.AddToClassList("hd-skill__cost"); _cost.AddToClassList("hd-skill__costtext"); _costBadge.Add(_cost);
            _slotBadge.AddToClassList("hd-skill__slot"); _slot.AddToClassList("hd-skill__slottext"); _slotBadge.Add(_slot);
            foreach (var e in new VisualElement[] { _ring, _art, _shade, _frame, _glyph, _plus, _label, _costBadge, _slotBadge }) { e.pickingMode = PickingMode.Ignore; _root.Add(e); }
            _root.RegisterCallback<PointerDownEvent>(OnDown);
            _root.RegisterCallback<PointerMoveEvent>(OnMove);
            _root.RegisterCallback<PointerUpEvent>(OnUp);
            _root.RegisterCallback<PointerCaptureOutEvent>(_ => { if (!_dragging) Release(); });
        }

        public void Show(SkillSlotData d, int index)
        {
            Data = d;
            _root.style.display = d.Visible ? DisplayStyle.Flex : DisplayStyle.None;
            if (!d.Visible) return;
            _slot.text = (index + 1).ToString();
            // Emplacement du grimoire encore vide : contour discret et icône « plus » du kit.
            _root.EnableInClassList("hd-skill--locked", d.Locked);
            _plus.style.display = d.Locked ? DisplayStyle.Flex : DisplayStyle.None;
            _glyph.text = d.Locked ? "" : d.Glyph;
            _label.text = d.Locked ? "" : NameOf(d.Label);
            _root.EnableInClassList("hd-skill--pending", d.Pending);
            if (_artKey != d.ArtKey) { _artKey = d.ArtKey; _cachedArt = AgentArt.Skill(d.ArtKey ?? ""); }
            var art = d.Locked ? null : _cachedArt;
            _art.style.display = art != null ? DisplayStyle.Flex : DisplayStyle.None;
            _art.style.backgroundImage = art != null ? new StyleBackground(Background.FromSprite(art)) : new StyleBackground(StyleKeyword.None);
            _shade.style.display = art != null ? DisplayStyle.Flex : DisplayStyle.None;
            _glyph.style.display = !d.Locked && art == null ? DisplayStyle.Flex : DisplayStyle.None;
            bool paid = !d.Locked && d.Cost > 0;
            _costBadge.style.display = paid ? DisplayStyle.Flex : DisplayStyle.None;
            _cost.text = paid ? d.Cost.ToString() : "";
            _cost.style.color = d.Affordable ? Theme.TextDark : Theme.Danger;
            bool ringOn = d.Enabled || d.Pending;
            _ring.style.display = ringOn ? DisplayStyle.Flex : DisplayStyle.None;
            _ring.style.borderTopColor = _ring.style.borderBottomColor = _ring.style.borderLeftColor = _ring.style.borderRightColor = d.Pending ? Theme.Primary : Theme.Glow;
            CanDrag = d.Enabled;
            _root.SetEnabled(true);
            _root.style.opacity = d.Locked ? 0.7f : d.Enabled || d.Pending ? 1f : 0.62f;
        }

        /// <summary>« [2] Barrière de fer » → « Barrière de fer » (le numéro est déjà affiché dans le coin).</summary>
        static string NameOf(string labelWithKey)
        {
            int close = labelWithKey.IndexOf(']');
            return close >= 0 ? labelWithKey.Substring(close + 1).Trim() : labelWithKey;
        }

        /// <summary>Pose la place de repos de la carte (rotation de l'éventail).</summary>
        public void SetHome(float rotateDegrees) { _homeRotate = rotateDegrees; HudAnim.SetRotate(_root, rotateDegrees); }

        // ---- Appui maintenu et glisser (pointeur capturé : le geste continue hors de la carte)
        void OnDown(PointerDownEvent e)
        {
            if (_down) return;
            _down = true; _pointer = e.pointerId; _startPanel = e.position;
            _root.CapturePointer(e.pointerId);
            Holding?.Invoke(this, true);
        }

        void OnMove(PointerMoveEvent e)
        {
            if (!_down || e.pointerId != _pointer) return;
            if (!_dragging)
            {
                if (!CanDrag || Vector2.Distance(e.position, _startPanel) < DragThreshold) return;
                BeginDrag();
            }
            Follow((Vector2)e.position - _startPanel);
        }

        void OnUp(PointerUpEvent e)
        {
            if (!_down || e.pointerId != _pointer) return;
            _root.ReleasePointer(e.pointerId);
            if (_dragging) EndDrag(HudSpace.PanelToScreen(_root, e.position));
            else Release();
        }

        void Release()
        {
            if (!_down) return;
            _down = false; _pointer = -1;
            Holding?.Invoke(this, false);
        }

        void BeginDrag()
        {
            _dragging = true;
            _root.BringToFront();
            HudAnim.SetScale(_root, 1.15f);
            HudAnim.SetRotate(_root, 4f);
            Holding?.Invoke(this, false);
            DragBegan?.Invoke(this);
        }

        void Follow(Vector2 delta) => HudAnim.SetTranslate(_root, delta.x, delta.y);

        void EndDrag(Vector2 screenAt)
        {
            _dragging = false; _down = false; _pointer = -1;
            var from = _root.resolvedStyle.translate;
            float scale = _root.resolvedStyle.scale.value.x, rot = _root.resolvedStyle.rotate.angle.value;
            if (HudAnim.Allowed)
            {
                HudAnim.Run(_root, 0.22f, k =>
                {
                    HudAnim.SetTranslate(_root, Mathf.LerpUnclamped(from.x, 0f, k), Mathf.LerpUnclamped(from.y, 0f, k));
                    HudAnim.SetScale(_root, Mathf.LerpUnclamped(scale, 1f, k));
                    HudAnim.SetRotate(_root, Mathf.LerpUnclamped(rot, _homeRotate, k));
                }, Home, HudAnim.EaseOutBack);
            }
            else Home();
            DragEnded?.Invoke(this, screenAt);
        }

        void Home()
        {
            HudAnim.SetTranslate(_root, 0f, 0f); HudAnim.SetScale(_root, 1f); HudAnim.SetRotate(_root, _homeRotate);
        }

        // ---- Gestes simulés (tests PlayMode) : mêmes chemins que le pointeur
        public void Press(bool down)
        {
            if (down) { if (_down) return; _down = true; Holding?.Invoke(this, true); }
            else Release();
        }

        public void SimulateDrag(Vector2 screenTo)
        {
            if (!CanDrag || _dragging) return;
            _down = true;
            BeginDrag();
            EndDrag(screenTo);
        }

        /// <summary>Un simple appui bref ne lance rien (les cartes se jouent au glisser-déposer).</summary>
        public void Tap() { Press(true); Press(false); }
    }
}
