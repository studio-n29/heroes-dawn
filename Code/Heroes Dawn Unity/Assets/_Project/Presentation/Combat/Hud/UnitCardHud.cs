#nullable enable
using System;
using N29.HeroesDawn.Presentation.Common;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Combat.Hud
{
    /// <summary>Carte d'une unité en combat (UI Toolkit) : portrait, nom, PV, jauge d'énergie, statuts, états (prête, sélectionnée, ciblable, K.O.).</summary>
    public sealed class UnitCardHud
    {
        readonly VisualElement _root = new VisualElement(), _portrait = new VisualElement(), _selectRing = new VisualElement(), _targetRing = new VisualElement(), _flash = new VisualElement();
        readonly VisualElement _hpFill = new VisualElement(), _gaugeFill = new VisualElement(), _floats = new VisualElement();
        readonly Label _initial = new Label(), _name = new Label(), _sub = new Label(), _hpText = new Label(), _statuses = new Label(), _ko = new Label();
        bool _ally;
        Color _baseFill;

        public VisualElement Root => _root;
        public int UnitId { get; private set; }
        /// <summary>Illustration de l'agent (null : silhouette provisoire), reprise en grand au centre du terrain quand il agit.</summary>
        public Sprite? PortraitArt { get; private set; }
        public event Action<int>? Tapped;

        public UnitCardHud()
        {
            _root.AddToClassList("hd-unit");
            _selectRing.AddToClassList("hd-unit__select"); _targetRing.AddToClassList("hd-unit__target");
            _selectRing.pickingMode = PickingMode.Ignore; _targetRing.pickingMode = PickingMode.Ignore;
            _selectRing.style.display = DisplayStyle.None; _targetRing.style.display = DisplayStyle.None;
            _portrait.AddToClassList("hd-unit__portrait");
            _initial.AddToClassList("hd-unit__initial");
            _portrait.Add(_initial);
            _name.AddToClassList("hd-unit__name"); _sub.AddToClassList("hd-unit__sub");
            var hp = new VisualElement(); hp.AddToClassList("hd-unit__bar"); _hpFill.AddToClassList("hd-unit__hp"); _hpText.AddToClassList("hd-unit__bartext");
            hp.Add(_hpFill); hp.Add(_hpText);
            var gauge = new VisualElement(); gauge.AddToClassList("hd-unit__gauge"); _gaugeFill.AddToClassList("hd-unit__gaugefill"); gauge.Add(_gaugeFill);
            _statuses.AddToClassList("hd-unit__statuses");
            _ko.AddToClassList("hd-unit__ko"); _ko.text = "K.O."; _ko.style.display = DisplayStyle.None;
            _flash.AddToClassList("hd-unit__flash"); _flash.pickingMode = PickingMode.Ignore;
            _floats.AddToClassList("hd-unit__floats"); _floats.pickingMode = PickingMode.Ignore;
            foreach (var e in new VisualElement[] { _targetRing, _selectRing, _portrait, _name, _sub, hp, gauge, _statuses, _flash, _ko, _floats }) _root.Add(e);
            _root.RegisterCallback<ClickEvent>(_ => Tapped?.Invoke(UnitId));
        }

        static void Border(VisualElement e, Color c)
            => e.style.borderTopColor = e.style.borderBottomColor = e.style.borderLeftColor = e.style.borderRightColor = c;

        public void Setup(UnitCardData d)
        {
            UnitId = d.Id; _ally = d.Ally;
            _root.name = (d.Ally ? "Ally_" : "Enemy_") + d.Id;
            _baseFill = d.Ally ? Theme.Surface : Color.Lerp(Theme.SurfaceDark, Theme.Danger, 0.16f);
            _root.style.backgroundColor = _baseFill;
            _sub.text = d.Sub ?? "";
            var family = Theme.FamilyColor(d.Family);
            Border(_portrait, family);
            PortraitArt = d.Portrait.Art;
            _portrait.style.backgroundImage = PortraitArt != null ? new StyleBackground(Background.FromSprite(PortraitArt)) : new StyleBackground(StyleKeyword.None);
            _portrait.style.backgroundColor = Color.Lerp(family, Theme.Navy, d.Ally ? 0.25f : 0.4f);
            _initial.text = PortraitArt != null ? "" : d.Portrait.Initial;
            _name.text = d.Name;
            _name.style.color = d.Boss ? Theme.Primary : Theme.Text;
            _hpFill.style.backgroundColor = d.Ally ? Theme.Hp : Theme.HpEnemy;
            _gaugeFill.style.backgroundColor = Theme.Info;
        }

        public void Refresh(UnitStateData s)
        {
            _hpFill.style.width = Length.Percent(Mathf.Clamp01(s.Hp01) * 100f);
            _hpText.text = s.HpText;
            _gaugeFill.style.width = Length.Percent(Mathf.Clamp01(s.Energy01) * 100f);
            _statuses.text = s.Statuses ?? "";
            _ko.style.display = s.Alive ? DisplayStyle.None : DisplayStyle.Flex;
            _root.style.opacity = s.Alive ? 1f : 0.45f;
            _root.style.backgroundColor = s.Highlight && s.Alive ? Color.Lerp(_baseFill, Theme.Primary, 0.22f) : _baseFill;
            _root.EnableInClassList("hd-unit--ready", s.Highlight && s.Alive);
            _selectRing.style.display = s.Selected && s.Alive ? DisplayStyle.Flex : DisplayStyle.None;
            _targetRing.style.display = s.Targetable && s.Alive ? DisplayStyle.Flex : DisplayStyle.None;
            Border(_targetRing, _ally ? Theme.Success : Theme.Danger);
        }

        public void Float(string text, Color color, int size)
        {
            var l = new Label(text) { pickingMode = PickingMode.Ignore };
            l.AddToClassList("hd-unit__float");
            l.style.color = color; l.style.fontSize = size;
            float x = UnityEngine.Random.Range(-60f, 60f);
            _floats.Add(l);
            l.style.marginLeft = x;
            HudAnim.PopIn(l, 0.18f);
            HudAnim.Run(l, 0.9f, k =>
            {
                l.style.translate = new Translate(0f, -90f * k);
                l.style.opacity = k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f;
            }, () => l.RemoveFromHierarchy());
        }

        public void Hit(VisualElement fxLayer)
        {
            HudAnim.Shake(_root, 12f, 0.25f);
            HudAnim.Flash(_flash);
            HudAnim.Sparks(fxLayer, _root, _ally ? Theme.Danger : Theme.Primary, 7, 90f, 14f);
        }

        public void Act()
        {
            HudAnim.Punch(_root, 0.08f, 0.25f);
            if (!HudAnim.Allowed) return;
            // Élan : la carte se penche vers son adversaire (alliés à gauche, ennemis au centre) puis revient.
            float lean = _ally ? -7f : 7f;
            HudAnim.Run(_root, 0.3f, k => HudAnim.SetRotate(_root, lean * Mathf.Sin(k * Mathf.PI)), () => HudAnim.SetRotate(_root, 0f));
        }

        public void FlashCard() => HudAnim.Flash(_flash);

        /// <summary>Centre de la carte en coordonnées de l'écran (y vers le haut), pour les dépôts et les tests.</summary>
        public Vector2 ScreenCenter() => HudSpace.PanelToScreen(_root, _root.worldBound.center);
        public bool ContainsScreenPoint(Vector2 screen) => _root.worldBound.Contains(HudSpace.ScreenToPanel(_root, screen));
    }
}
