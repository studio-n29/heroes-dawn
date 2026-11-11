#nullable enable
using System;
using System.Collections.Generic;
using N29.HeroesDawn.Presentation.Combat.Hud;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.Activities
{
    /// <summary>Zone de jeu d'un mini-jeu 2D (UI Toolkit) : les pilotes y créent des <see cref="FieldSprite"/> placés comme avant (origine au centre, y vers le haut).</summary>
    public sealed class FieldSurface
    {
        public VisualElement Element { get; }
        readonly List<FieldSprite> _owned = new List<FieldSprite>();

        public FieldSurface(VisualElement element) => Element = element;

        /// <summary>Taille de la zone ; avant la première mise en page, une taille par défaut.</summary>
        public Vector2 Size { get { var s = Element.layout.size; return s.x > 2f && s.y > 2f ? s : new Vector2(1208f, 604f); } }

        /// <summary>Rectangle de la zone, origine au centre (comme l'ancien RectTransform).</summary>
        public Rect rect { get { var s = Size; return new Rect(-s.x * 0.5f, -s.y * 0.5f, s.x, s.y); } }

        public FieldSprite Add(string name, Sprite? sprite, Color color, Vector2 size)
        {
            var s = new FieldSprite(this, name, sprite, color, size);
            _owned.Add(s);
            Element.Add(s.Element);
            return s;
        }

        public void Clear()
        {
            foreach (var s in _owned) s.Element.RemoveFromHierarchy();
            _owned.Clear();
        }
    }

    /// <summary>Calque d'activation d'un élément (forme de « gameObject.SetActive »).</summary>
    public sealed class FieldGO
    {
        readonly VisualElement _e;
        public FieldGO(VisualElement e) => _e = e;
        public void SetActive(bool on) => _e.style.display = on ? DisplayStyle.Flex : DisplayStyle.None;
        public bool activeSelf => _e.style.display != DisplayStyle.None;
        public FieldGO gameObject => this;
    }

    /// <summary>Position, taille et rotation d'un élément de la zone, dans les repères de l'ancien RectTransform (origine au centre, y vers le haut, rotation anti-horaire).</summary>
    public sealed class FieldRect
    {
        readonly FieldSprite _s;
        public FieldRect(FieldSprite s) => _s = s;
        public Vector2 anchoredPosition { get => _s.Position; set => _s.SetPosition(value); }
        public Vector2 sizeDelta { get => _s.Size; set => _s.SetSize(value); }
        public Quaternion localRotation { set => _s.SetRotation(value.eulerAngles.z); }
        public Vector2 pivot { get => _s.Pivot; set => _s.SetPivot(value); }
    }

    /// <summary>Image d'un mini-jeu 2D (UI Toolkit) : sprite teinté, ou aplat de couleur sans sprite.</summary>
    public sealed class FieldSprite
    {
        readonly FieldSurface _surface;
        Sprite? _sprite;
        Color _color;

        public VisualElement Element { get; } = new VisualElement();
        public Vector2 Position { get; private set; }
        public Vector2 Pivot { get; private set; } = new Vector2(0.5f, 0.5f);
        public void SetPivot(Vector2 p) { Pivot = p; Element.style.transformOrigin = new TransformOrigin(Length.Percent(p.x * 100f), Length.Percent((1f - p.y) * 100f)); Place(); }
        public Vector2 Size { get; private set; }
        public FieldRect rectTransform { get; }
        public FieldGO gameObject { get; }

        public FieldSprite(FieldSurface surface, string name, Sprite? sprite, Color color, Vector2 size)
        {
            _surface = surface; _sprite = sprite; _color = color; Size = size;
            Element.name = name;
            Element.pickingMode = PickingMode.Ignore;
            Element.style.position = Position_.Absolute;
            rectTransform = new FieldRect(this);
            gameObject = new FieldGO(Element);
            Apply(); Place();
        }

        static class Position_ { public const UnityEngine.UIElements.Position Absolute = UnityEngine.UIElements.Position.Absolute; }

        public Sprite? sprite { get => _sprite; set { _sprite = value; Apply(); } }
        public Color color { get => _color; set { _color = value; Apply(); } }
        /// <summary>Les éléments de la zone ne captent jamais le toucher (la zone reçoit tout) : sans effet.</summary>
        public bool raycastTarget { get; set; }

        void Apply()
        {
            if (_sprite != null)
            {
                Element.style.backgroundImage = new StyleBackground(Background.FromSprite(_sprite));
                Element.style.backgroundColor = Color.clear;
                Element.style.unityBackgroundImageTintColor = _color;
            }
            else
            {
                Element.style.backgroundImage = new StyleBackground(StyleKeyword.None);
                Element.style.backgroundColor = _color;
            }
        }

        public void SetPosition(Vector2 p) { Position = p; Place(); }
        public void SetSize(Vector2 s) { Size = s; Element.style.width = s.x; Element.style.height = s.y; Place(); }
        public void SetRotation(float zDegrees) => HudAnim.SetRotate(Element, -zDegrees);

        void Place()
        {
            var area = _surface.Size;
            Element.style.width = Size.x; Element.style.height = Size.y;
            Element.style.left = area.x * 0.5f + Position.x - Size.x * Pivot.x;
            Element.style.top = area.y * 0.5f - Position.y - Size.y * (1f - Pivot.y);
        }

        /// <summary>Replace l'élément (la zone a changé de taille).</summary>
        public void Relayout() => Place();
    }

    /// <summary>Texte d'un panneau (forme de « texte.text »).</summary>
    public sealed class TextRef
    {
        readonly Label _l;
        public TextRef(Label l) { _l = l; l.enableRichText = true; }
        public string text { get => _l.text; set => _l.text = value; }
    }

    /// <summary>Liste de réactions d'un bouton (forme de « onClick.AddListener »).</summary>
    public sealed class ClickList
    {
        readonly UnityEngine.UIElements.Button _b;
        readonly List<Action> _actions = new List<Action>();
        public ClickList(UnityEngine.UIElements.Button b) { _b = b; b.clicked += () => { foreach (var a in _actions.ToArray()) a(); }; }
        public void AddListener(Action a) => _actions.Add(a);
        public void RemoveAllListeners() => _actions.Clear();
        public void Invoke() { foreach (var a in _actions.ToArray()) a(); }
    }

    /// <summary>Bouton du panneau de secours (forme des anciens boutons habillés).</summary>
    public sealed class HudBtn
    {
        readonly UnityEngine.UIElements.Button _b;
        public HudBtn(UnityEngine.UIElements.Button b) { _b = b; onClick = new ClickList(b); gameObject = new FieldGO(b); }
        public UnityEngine.UIElements.Button Element => _b;
        public HudBtn Button => this;
        public ClickList onClick { get; }
        public FieldGO gameObject { get; }
        public bool Interactable { get => _b.enabledSelf; set => _b.SetEnabled(value); }
        /// <summary>Avant le début, un bouton ne capte pas le toucher : le premier appui lance la partie.</summary>
        public HudBtn Fill => this;
        public bool raycastTarget { set => _b.pickingMode = value ? PickingMode.Position : PickingMode.Ignore; }
        public void SetLabel(string text) => _b.text = text;
    }

    /// <summary>
    /// Interface d'« Ingénierie de secours » (UI Toolkit, masquée hors de ce jeu) : le manuel, la réserve de pièces, les emplacements de l'outil,
    /// Annuler et Valider. Le pilote ne fait que la remplir.
    /// </summary>
    public sealed class RescuePanelHud
    {
        public const int PoolSize = 16, SlotSize = 8;
        public VisualElement Root { get; } = new VisualElement();
        readonly Label _manual = new Label(), _status = new Label();
        readonly List<HudBtn> _pool = new List<HudBtn>();
        readonly List<VisualElement> _boxes = new List<VisualElement>();
        readonly List<TextRef> _slotTexts = new List<TextRef>();

        public TextRef Manual { get; }
        public TextRef Status { get; }
        public HudBtn Undo { get; }
        public HudBtn Submit { get; }
        public int PoolCapacity => PoolSize;
        public int SlotCapacity => SlotSize;
        public HudBtn PoolButton(int i) => _pool[i];
        public FieldGO SlotBox(int i) => new FieldGO(_boxes[i]);
        public TextRef SlotText(int i) => _slotTexts[i];

        public RescuePanelHud()
        {
            Root.AddToClassList("hd-rescue");
            var left = new VisualElement(); left.AddToClassList("hd-rescue__left");
            _manual.AddToClassList("hd-rescue__manual"); _status.AddToClassList("hd-rescue__status");
            Manual = new TextRef(_manual); Status = new TextRef(_status);
            var slots = new VisualElement(); slots.AddToClassList("hd-rescue__slots");
            for (int i = 0; i < SlotSize; i++)
            {
                var box = new VisualElement(); box.AddToClassList("hd-rescue__slot");
                var t = new Label(); t.AddToClassList("hd-rescue__slottext"); box.Add(t);
                slots.Add(box); _boxes.Add(box); _slotTexts.Add(new TextRef(t));
            }
            left.Add(_manual); left.Add(slots); left.Add(_status);
            var right = new VisualElement(); right.AddToClassList("hd-rescue__right");
            var pool = new VisualElement(); pool.AddToClassList("hd-rescue__pool");
            for (int i = 0; i < PoolSize; i++)
            {
                var b = new UnityEngine.UIElements.Button(); b.AddToClassList("hd-btn"); b.AddToClassList("hd-btn--secondary"); b.AddToClassList("hd-rescue__part");
                pool.Add(b); _pool.Add(new HudBtn(b));
            }
            var row = new VisualElement(); row.AddToClassList("hd-rescue__actions");
            var undo = new UnityEngine.UIElements.Button(); undo.AddToClassList("hd-btn"); undo.AddToClassList("hd-btn--secondary");
            var submit = new UnityEngine.UIElements.Button(); submit.AddToClassList("hd-btn"); submit.AddToClassList("hd-btn--primary");
            row.Add(undo); row.Add(submit);
            Undo = new HudBtn(undo); Submit = new HudBtn(submit);
            right.Add(pool); right.Add(row);
            Root.Add(left); Root.Add(right);
            Root.style.display = DisplayStyle.None;
        }
    }
}
