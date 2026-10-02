using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    public enum FontKind { Title, Body, Regular }

    /// <summary>
    /// Fabrique d'éléments uGUI construits par code (aucun prefab requis).
    /// Style « comics » (référence : Secret Wars 2015) : cadres à contour noir épais, ombre portée décalée,
    /// titres en Bangers, textes courants en Comic Neue.
    /// </summary>
    public static class Ui
    {
        static Font _title, _body, _regular, _fallback;

        public static Font Fallback => _fallback != null ? _fallback : (_fallback = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"));
        public static Font TitleFont => _title != null ? _title : (_title = LoadFont("Fonts/Bangers-Regular"));
        public static Font BodyFont => _body != null ? _body : (_body = LoadFont("Fonts/ComicNeue-Bold"));
        public static Font RegularFont => _regular != null ? _regular : (_regular = LoadFont("Fonts/ComicNeue-Regular"));

        static Font LoadFont(string path)
        {
            var f = Resources.Load<Font>(path);
            if (f == null) Debug.LogWarning("[Heroes Dawn] Police introuvable : Resources/" + path + " — police par défaut utilisée.");
            return f != null ? f : Fallback;
        }

        public static Font FontFor(FontKind k) => k == FontKind.Title ? TitleFont : k == FontKind.Body ? BodyFont : RegularFont;

        // ---------------------------------------------------------------- Rects

        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = 5; // UI
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        /// <summary>Étire dans le parent avec des marges (gauche, bas, droite, haut).</summary>
        public static RectTransform Stretch(this RectTransform rt, float l = 0, float b = 0, float r = 0, float t = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(l, b);
            rt.offsetMax = new Vector2(-r, -t);
            return rt;
        }

        /// <summary>Place à un point d'ancrage (ax, ay ∈ [0,1], ancre = pivot) avec une position et une taille.</summary>
        public static RectTransform At(this RectTransform rt, float ax, float ay, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = new Vector2(ax, ay);
            rt.pivot = new Vector2(ax, ay);
            rt.anchoredPosition = new Vector2(x, y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        /// <summary>Bande horizontale en haut (h) ou en bas (h &lt; 0) du parent.</summary>
        public static RectTransform TopBand(this RectTransform rt, float height, float margin = 0)
        {
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(1, 1);
            rt.pivot = new Vector2(0.5f, 1);
            rt.anchoredPosition = new Vector2(0, -margin);
            rt.sizeDelta = new Vector2(-2 * margin, height);
            return rt;
        }

        public static RectTransform BottomBand(this RectTransform rt, float height, float margin = 0)
        {
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, margin);
            rt.sizeDelta = new Vector2(-2 * margin, height);
            return rt;
        }

        // ---------------------------------------------------------------- Graphics

        public static Image Img(Transform parent, string name, Color color, Sprite sprite = null, bool raycast = false)
        {
            var rt = Rect(parent, name);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            if (sprite != null)
            {
                img.sprite = sprite;
                img.type = sprite.border != Vector4.zero ? Image.Type.Sliced : Image.Type.Simple;
            }
            return img;
        }

        /// <summary>
        /// Case de BD : contour noir (épaisseur <paramref name="border"/>), fond coloré, ombre portée.
        /// Renvoie le fond intérieur ; son parent (le contour) est le RectTransform à placer.
        /// </summary>
        public static Image Panel(Transform parent, string name, Color fill, float border = 4f, float shadow = 8f, bool raycast = true)
        {
            var frame = Img(parent, name, Theme.Ink, Sprites.Rounded, raycast);
            if (shadow > 0)
            {
                var sh = frame.gameObject.AddComponent<Shadow>();
                sh.effectColor = new Color(0, 0, 0, 0.55f);
                sh.effectDistance = new Vector2(shadow, -shadow);
            }
            var inner = Img(frame.transform, "Fill", fill, Sprites.Rounded, false);
            inner.rectTransform.Stretch(border, border, border, border);
            return inner;
        }

        public static RectTransform Frame(this Image panelFill) => (RectTransform)panelFill.transform.parent;

        public static Text Txt(Transform parent, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter,
                               FontKind font = FontKind.Body, string name = "Text")
        {
            var rt = Rect(parent, name);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = FontFor(font);
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.text = text;
            t.supportRichText = true;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.lineSpacing = font == FontKind.Title ? 0.9f : 1f;
            return t;
        }

        /// <summary>Contour noir « lettrage de couverture » sur un texte.</summary>
        public static T Inked<T>(this T g, float size = 2.5f) where T : Graphic
        {
            var o = g.gameObject.AddComponent<Outline>();
            o.effectColor = Theme.Ink;
            o.effectDistance = new Vector2(size, -size);
            var s = g.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(0, 0, 0, 0.8f);
            s.effectDistance = new Vector2(size + 2, -(size + 2));
            return g;
        }

        public static UiButton Btn(Transform parent, string label, Color bg, Color fg, Action onClick, int fontSize = 34,
                                   FontKind font = FontKind.Title, string name = "Button")
        {
            var fill = Panel(parent, name, bg, 4f, 6f, true);
            var frameGo = fill.transform.parent.gameObject;
            var button = frameGo.AddComponent<Button>();
            button.targetGraphic = fill;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.45f, 0.45f, 0.45f, 0.9f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;

            var text = Txt(fill.transform, label, fontSize, fg, TextAnchor.MiddleCenter, font, "Label");
            text.rectTransform.Stretch(10, 4, 10, 4);
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.resizeTextForBestFit = true;
            text.resizeTextMinSize = Mathf.Max(12, fontSize / 2);
            text.resizeTextMaxSize = fontSize;

            var ub = frameGo.AddComponent<UiButton>();
            ub.Init(button, fill, text, bg, fg);
            if (onClick != null) button.onClick.AddListener(() => onClick());
            return ub;
        }

        public static UiBar Bar(Transform parent, string name, Color fill, Color back, bool withLabel = false, int labelSize = 18)
        {
            var bg = Img(parent, name, Theme.Ink, Sprites.RoundedSmall);
            var inner = Img(bg.transform, "Back", back, Sprites.RoundedSmall);
            inner.rectTransform.Stretch(2, 2, 2, 2);
            var f = Img(inner.transform, "Fill", fill, Sprites.RoundedSmall);
            var frt = f.rectTransform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;
            Text label = null;
            if (withLabel)
            {
                label = Txt(bg.transform, "", labelSize, Color.white, TextAnchor.MiddleCenter, FontKind.Body, "Label");
                label.rectTransform.Stretch();
                label.Inked(1.5f);
            }
            var bar = bg.gameObject.AddComponent<UiBar>();
            bar.Init(frt, f, label);
            return bar;
        }

        public static HorizontalLayoutGroup HLayout(this RectTransform rt, float spacing, int pad = 0,
                                                    TextAnchor align = TextAnchor.MiddleCenter, bool expandWidth = false)
        {
            var h = rt.gameObject.AddComponent<HorizontalLayoutGroup>();
            h.spacing = spacing;
            h.padding = new RectOffset(pad, pad, pad, pad);
            h.childAlignment = align;
            h.childControlWidth = true;
            h.childControlHeight = true;
            h.childForceExpandWidth = expandWidth;
            h.childForceExpandHeight = false;
            return h;
        }

        public static VerticalLayoutGroup VLayout(this RectTransform rt, float spacing, int pad = 0,
                                                  TextAnchor align = TextAnchor.UpperCenter, bool expandHeight = false)
        {
            var v = rt.gameObject.AddComponent<VerticalLayoutGroup>();
            v.spacing = spacing;
            v.padding = new RectOffset(pad, pad, pad, pad);
            v.childAlignment = align;
            v.childControlWidth = true;
            v.childControlHeight = true;
            v.childForceExpandWidth = true;
            v.childForceExpandHeight = expandHeight;
            return v;
        }

        /// <summary>Taille préférée pour un élément enfant d'un groupe de disposition (-1 = libre).</summary>
        public static LayoutElement Pref(this Component c, float w, float h, float flexW = -1, float flexH = -1)
        {
            var le = c.GetComponent<LayoutElement>();
            if (le == null) le = c.gameObject.AddComponent<LayoutElement>();
            le.preferredWidth = w;
            le.preferredHeight = h;
            if (w >= 0) le.minWidth = w;
            if (h >= 0) le.minHeight = h;
            le.flexibleWidth = flexW;
            le.flexibleHeight = flexH;
            return le;
        }

        /// <summary>Zone défilante verticale (masque + contenu auto-dimensionné). Renvoie le contenu où ajouter les éléments.</summary>
        public static RectTransform VScroll(RectTransform parent, out ScrollRect scroll)
        {
            var view = Rect(parent, "Scroll").Stretch();
            var hit = view.gameObject.AddComponent<Image>();
            hit.color = new Color(0, 0, 0, 0);
            view.gameObject.AddComponent<RectMask2D>();
            scroll = view.gameObject.AddComponent<ScrollRect>();
            var content = Rect(view, "Content");
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1);
            content.anchoredPosition = Vector2.zero;
            content.sizeDelta = Vector2.zero;
            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.content = content;
            scroll.viewport = view;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;
            return content;
        }

        /// <summary>Supprime tous les enfants (reconstruction de listes).</summary>
        public static void Clear(this Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--)
            {
                var c = t.GetChild(i).gameObject;
                c.SetActive(false);
                UnityEngine.Object.Destroy(c);
            }
        }

        /// <summary>Zone cliquable invisible (pour détecter un toucher sur un élément non bouton).</summary>
        public static Button MakeClickable(this Graphic g, Action onClick)
        {
            g.raycastTarget = true;
            var b = g.gameObject.GetComponent<Button>();
            if (b == null) b = g.gameObject.AddComponent<Button>();
            b.transition = Selectable.Transition.None;
            var nav = b.navigation;
            nav.mode = Navigation.Mode.None;
            b.navigation = nav;
            b.onClick.AddListener(() => onClick());
            return b;
        }
    }

    /// <summary>Bouton « comics » : léger écrasement à l'appui.</summary>
    public sealed class UiButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public Button Button { get; private set; }
        public Image Fill { get; private set; }
        public Text Label { get; private set; }
        public RectTransform Rect => (RectTransform)transform;
        Color _bg;
        Color _fg;

        public void Init(Button b, Image fill, Text label, Color bg, Color fg)
        {
            Button = b; Fill = fill; Label = label; _bg = bg; _fg = fg;
            fill.color = bg;
        }

        public bool Interactable
        {
            get => Button.interactable;
            set
            {
                Button.interactable = value;
                Label.color = value ? _fg : Theme.WithAlpha(_fg, 0.55f);
            }
        }

        public void SetLabel(string s) => Label.text = s;

        public void SetColors(Color bg, Color fg)
        {
            _bg = bg; _fg = fg;
            Fill.color = bg;
            Label.color = Button.interactable ? fg : Theme.WithAlpha(fg, 0.55f);
        }

        public void OnPointerDown(PointerEventData e) { if (Button.interactable) transform.localScale = new Vector3(0.95f, 0.95f, 1f); }
        public void OnPointerUp(PointerEventData e) => transform.localScale = Vector3.one;
        public void OnPointerExit(PointerEventData e) => transform.localScale = Vector3.one;
        void OnDisable() => transform.localScale = Vector3.one;
    }

    /// <summary>Barre de progression (PV, jauge ATB, fatigue) avec interpolation douce.</summary>
    public sealed class UiBar : MonoBehaviour
    {
        RectTransform _fill;
        Image _fillImage;
        Text _label;
        float _target = 1f;
        float _shown = 1f;
        public bool Smooth = true;
        public float Speed = 2.5f;

        public Text Label => _label;

        public void Init(RectTransform fill, Image fillImage, Text label)
        {
            _fill = fill; _fillImage = fillImage; _label = label;
            Apply();
        }

        public void Set(float ratio, bool instant = false)
        {
            _target = Mathf.Clamp01(ratio);
            if (instant || !Smooth || !isActiveAndEnabled) { _shown = _target; Apply(); }
        }

        public void SetColor(Color c) => _fillImage.color = c;
        public void SetLabel(string s) { if (_label != null) _label.text = s; }

        void Update()
        {
            if (Mathf.Abs(_shown - _target) < 0.0005f) return;
            _shown = Mathf.MoveTowards(_shown, _target, Time.unscaledDeltaTime * Speed);
            Apply();
        }

        void Apply()
        {
            if (_fill == null) return;
            _fill.anchorMax = new Vector2(_shown, 1f);
            _fillImage.enabled = _shown > 0.001f;
        }
    }
}
