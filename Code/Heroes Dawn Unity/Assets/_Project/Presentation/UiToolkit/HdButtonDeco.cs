#nullable enable
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Presentation.UiToolkit
{
    /// <summary>
    /// Ornements du bouton principal (« hd-btn--primary ») : une étoile à quatre branches à gauche du libellé et un chevron à droite,
    /// comme sur les planches. USS n'a ni pseudo-élément ni second fond : les deux ornements sont des enfants du bouton, posés ici une fois
    /// pour toutes les vues (<see cref="HdView"/> appelle <see cref="Apply"/> à l'activation puis à intervalle lent pour les boutons créés
    /// ou basculés après coup). Leur affichage reste piloté par USS : masqués hors bouton principal, sur « hd-btn--small » et « hd-btn--plain ».
    /// </summary>
    public static class HdButtonDeco
    {
        const string ButtonClass = "hd-btn", PrimaryClass = "hd-btn--primary", SmallClass = "hd-btn--small", PlainClass = "hd-btn--plain";
        const string DecoClass = "hd-btn__deco", StarClass = "hd-btn__star", ChevronClass = "hd-btn__chevron", WiredClass = "hd-btn--deco";
        // Tailles des ornements (celles de hd.uss) et marges autour du libellé, en pixels de référence.
        const float StarSize = 36f, ChevronSize = 28f, Gap = 14f, Inset = 14f, ChevronInset = 20f;

        /// <summary>Pose les ornements sur les boutons principaux de <paramref name="root"/> (lui compris) ; sans effet sur ceux déjà ornés.</summary>
        public static void Apply(VisualElement? root)
        {
            if (root == null) return;
            if (root is Button button && button.ClassListContains(ButtonClass))
            {
                if (button.ClassListContains(PrimaryClass) && !button.ClassListContains(SmallClass) && !button.ClassListContains(PlainClass)) Decorate(button);
                FitWidth(button);
                return;
            }
            var children = root.hierarchy;
            for (int i = 0, n = children.childCount; i < n; i++) Apply(children[i]);
        }

        /// <summary>
        /// UI Toolkit ne mesure le texte d'un élément que s'il n'a aucun enfant : dès qu'un bouton porte une icône, une étoile ou un chevron,
        /// sa largeur automatique se réduit à ses marges (filtres écrasés, « Passer le tutoriel » écrit lettre par lettre, retour coupé).
        /// On pose donc une largeur minimale égale à la largeur du libellé plus les marges. Ignoré pour un libellé volontairement invisible
        /// (boutons à contenu composé), vide, ou tant que la mise en page n'est pas calculée.
        /// </summary>
        static void FitWidth(Button button)
        {
            if (button.hierarchy.childCount == 0 || string.IsNullOrEmpty(button.text) || button.panel == null) return;
            var style = button.resolvedStyle;
            if (float.IsNaN(style.paddingLeft) || style.color.a < 0.05f || style.fontSize < 2f) return;
            float text = button.MeasureTextSize(button.text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x;
            if (float.IsNaN(text) || text <= 0f) return;
            float need = Mathf.Ceil(text + style.paddingLeft + style.paddingRight + style.borderLeftWidth + style.borderRightWidth + 2f);
            var current = button.style.minWidth;
            if (current.keyword == StyleKeyword.Undefined && Mathf.Abs(current.value.value - need) < 1f) return;
            button.style.minWidth = need;
        }

        static VisualElement? Find(Button button, string className)
        {
            var children = button.hierarchy;
            for (int i = 0, n = children.childCount; i < n; i++)
                if (children[i].ClassListContains(className)) return children[i];
            return null;
        }

        static void Decorate(Button button)
        {
            var star = Find(button, StarClass);
            if (star == null)
            {
                star = new VisualElement { pickingMode = PickingMode.Ignore };
                star.AddToClassList(DecoClass); star.AddToClassList(StarClass);
                var chevron = new VisualElement { pickingMode = PickingMode.Ignore };
                chevron.AddToClassList(DecoClass); chevron.AddToClassList(ChevronClass);
                button.hierarchy.Add(star); button.hierarchy.Add(chevron);
                // Les rappels ne se branchent qu'une fois par bouton, même si une vue vide puis reconstruit son contenu.
                if (!button.ClassListContains(WiredClass))
                {
                    button.AddToClassList(WiredClass);
                    button.RegisterCallback<GeometryChangedEvent>(OnGeometry);
                    button.RegisterCallback<ChangeEvent<string>>(OnText);
                }
                Place(button);
                return;
            }
            // Le texte a changé sans événement (bouton hors panneau au moment du changement) : on replace.
            if (!(star.userData is string placed) || placed != button.text) Place(button);
        }

        static void OnGeometry(GeometryChangedEvent e) { if (e.currentTarget is Button b) Place(b); }
        static void OnText(ChangeEvent<string> e) { if (e.currentTarget is Button b && e.target == e.currentTarget) Place(b); }

        /// <summary>Place l'étoile juste à gauche du libellé centré ; chaque ornement se cache quand la place manque (bouton étroit, libellé long, libellé non centré).</summary>
        static void Place(Button button)
        {
            var star = Find(button, StarClass);
            var chevron = Find(button, ChevronClass);
            if (star == null || chevron == null) return;
            string text = button.text ?? "";
            star.userData = text;
            float width = button.layout.width;
            if (button.panel == null || float.IsNaN(width) || width <= 0f) return;
            var align = button.resolvedStyle.unityTextAlign;
            bool centered = align == TextAnchor.MiddleCenter || align == TextAnchor.UpperCenter || align == TextAnchor.LowerCenter;
            if (text.Length == 0 || !centered)
            {
                star.style.visibility = Visibility.Hidden; chevron.style.visibility = Visibility.Hidden;
                return;
            }
            var content = button.contentRect;
            float textWidth = button.MeasureTextSize(text, 0f, VisualElement.MeasureMode.Undefined, 0f, VisualElement.MeasureMode.Undefined).x;
            float textLeft = content.x + (content.width - textWidth) * 0.5f, textRight = textLeft + textWidth;
            float starLeft = textLeft - Gap - StarSize;
            bool starFits = starLeft >= Inset;
            star.style.visibility = starFits ? Visibility.Visible : Visibility.Hidden;
            if (starFits) star.style.left = starLeft;
            bool chevronFits = width - textRight >= ChevronInset + ChevronSize + Gap;
            chevron.style.visibility = chevronFits ? Visibility.Visible : Visibility.Hidden;
        }
    }
}
