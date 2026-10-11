using System;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

namespace N29.HeroesDawn.Presentation.Common
{
    public enum FontKind { Title, Body, Regular }

    /// <summary>
    /// Fabrique d'éléments uGUI construits par code (aucun prefab requis).
    /// Style « HUD pastel » (spec 2026-10-12, référence : Marvel's Spider-Man) : panneaux de verre givré à coins coupés,
    /// filet clair, ombre douce, accent corail.
    /// </summary>
    public static class Ui
    {
        static TMP_FontAsset _title, _body, _regular;
        static readonly System.Collections.Generic.Dictionary<TMP_FontAsset, Material> _inked = new System.Collections.Generic.Dictionary<TMP_FontAsset, Material>();

        // Polices TextMeshPro générées (Heroes Dawn ▸ Setup ▸ Générer les assets d'interface → Resources/Fonts/TMP).
        public static TMP_FontAsset TitleFont => _title != null ? _title : (_title = LoadFont("Bangers-Regular"));
        public static TMP_FontAsset BodyFont => _body != null ? _body : (_body = LoadFont("ComicNeue-Bold"));
        public static TMP_FontAsset RegularFont => _regular != null ? _regular : (_regular = LoadFont("ComicNeue-Regular"));

        static TMP_FontAsset LoadFont(string name)
        {
            var f = Resources.Load<TMP_FontAsset>("Fonts/TMP/" + name + " SDF");
            if (f == null) Debug.LogWarning("[Heroes Dawn] Police TMP introuvable : Resources/Fonts/TMP/" + name + " SDF — police par défaut utilisée.");
            return f != null ? f : TMP_Settings.defaultFontAsset;
        }

        public static TMP_FontAsset FontFor(FontKind k) => k == FontKind.Title ? TitleFont : k == FontKind.Body ? BodyFont : RegularFont;

        /// <summary>Matériau « encré » (contour noir + ombre) de la police, ou son matériau par défaut s'il manque.</summary>
        public static Material InkedMaterial(TMP_FontAsset font)
        {
            if (!_inked.TryGetValue(font, out var m) || m == null) _inked[font] = m = Resources.Load<Material>("Fonts/TMP/" + font.name + " Inked");
            return m != null ? m : font.material;
        }

        public static TextAlignmentOptions Align(TextAnchor a)
        {
            switch (a)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleLeft: return TextAlignmentOptions.Left;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Center;
            }
        }

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
            // HUD pastel : filet clair (FrostEdge) à coins coupés, fond teinté, ombre douce (jamais d'ombre dure ni de contour noir).
            var frame = Img(parent, name, Theme.FrostEdge, Sprites.Panel, raycast);
            if (shadow > 0)
            {
                var sh = frame.gameObject.AddComponent<Shadow>();
                sh.effectColor = new Color(Theme.Navy.r, Theme.Navy.g, Theme.Navy.b, 0.25f);
                sh.effectDistance = new Vector2(shadow * 0.5f, -shadow * 0.5f);
            }
            float edge = Mathf.Min(border, 2f);
            var inner = Img(frame.transform, "Fill", fill, Sprites.Panel, false);
            inner.rectTransform.Stretch(edge, edge, edge, edge);
            return inner;
        }

        /// <summary>Barre d'accent verticale (4 px) sur le bord gauche d'un panneau : signature du HUD. Corail par défaut.</summary>
        public static Image AccentBar(Image panelFill, Color? color = null)
        {
            var bar = Img(panelFill.transform, "AccentBar", color ?? Theme.Accent);
            var rt = bar.rectTransform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = new Vector2(4f, -20f);
            return bar;
        }

        public static RectTransform Frame(this Image panelFill) => (RectTransform)panelFill.transform.parent;

        public static TextMeshProUGUI Txt(Transform parent, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter,
                                          FontKind font = FontKind.Body, string name = "Text")
        {
            var rt = Rect(parent, name);
            var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
            t.font = FontFor(font);
            t.fontSize = size;
            t.color = color;
            t.alignment = Align(align);
            t.text = text;
            t.richText = true;
            t.raycastTarget = false;
            t.textWrappingMode = TextWrappingModes.Normal;
            t.overflowMode = TextOverflowModes.Overflow;
            t.lineSpacing = font == FontKind.Title ? -10f : 0f;
            return t;
        }

        /// <summary>Texte statique localisé : la clé est portée par <see cref="Mvp.LocalizedText"/> et résolue à l'activation.</summary>
        public static TextMeshProUGUI LocTxt(Transform parent, string key, int size, Color color, TextAnchor align = TextAnchor.MiddleCenter,
                                             FontKind font = FontKind.Body, string name = "Text")
        {
            var t = Txt(parent, Loc.T(key), size, color, align, font, name);
            t.gameObject.AddComponent<Mvp.LocalizedText>().Key = key;
            return t;
        }

        /// <summary>
        /// Contour noir « lettrage de couverture » : matériau encré partagé pour un texte TMP (l'épaisseur est celle du matériau),
        /// contour + ombre uGUI pour un autre graphique.
        /// </summary>
        public static T Inked<T>(this T g, float size = 2.5f) where T : Graphic
        {
            if (g is TMP_Text tmp) { tmp.fontSharedMaterial = InkedMaterial(tmp.font); return g; }
            var s = g.gameObject.AddComponent<Shadow>();
            s.effectColor = new Color(Theme.Navy.r, Theme.Navy.g, Theme.Navy.b, 0.45f);
            s.effectDistance = new Vector2(size * 0.5f, -size * 0.5f);
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

            if (bg == Theme.Accent)
            {
                // Bouton d'action : reflet diagonal clair sur le dégradé corail.
                var shine = Img(fill.transform, "Shine", new Color(1f, 1f, 1f, 0.35f), Sprites.Shine);
                shine.rectTransform.Stretch(0, 0, 0, 0);
                shine.type = Image.Type.Simple;
            }
            var text = Txt(fill.transform, label, fontSize, fg, TextAnchor.MiddleCenter, font, "Label");
            text.rectTransform.Stretch(10, 4, 10, 4);
            text.enableAutoSizing = true;
            text.fontSizeMin = Mathf.Min(fontSize, Mathf.Max(20, fontSize / 2));
            text.fontSizeMax = fontSize;

            var ub = frameGo.AddComponent<UiButton>();
            ub.Init(button, fill, text, bg, fg);
            if (onClick != null) button.onClick.AddListener(() => onClick());
            return ub;
        }

        /// <summary>Bouton à libellé statique localisé (clé portée par <see cref="Mvp.LocalizedText"/>).</summary>
        public static UiButton LocBtn(Transform parent, string key, Color bg, Color fg, Action onClick, int fontSize = 34,
                                      FontKind font = FontKind.Title, string name = "Button")
        {
            var b = Btn(parent, Loc.T(key), bg, fg, onClick, fontSize, font, name);
            b.Label.gameObject.AddComponent<Mvp.LocalizedText>().Key = key;
            return b;
        }

        public static UiBar Bar(Transform parent, string name, Color fill, Color back, bool withLabel = false, int labelSize = 20)
        {
            var bg = Img(parent, name, Theme.FrostEdge, Sprites.PanelSmall);
            var inner = Img(bg.transform, "Back", back, Sprites.PanelSmall);
            inner.rectTransform.Stretch(1, 1, 1, 1);
            var f = Img(inner.transform, "Fill", fill, Sprites.PanelSmall);
            var frt = f.rectTransform;
            frt.anchorMin = Vector2.zero;
            frt.anchorMax = Vector2.one;
            frt.offsetMin = Vector2.zero;
            frt.offsetMax = Vector2.zero;
            TMP_Text label = null;
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

}
