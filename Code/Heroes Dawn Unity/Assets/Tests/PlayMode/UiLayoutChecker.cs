#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>
    /// Contrôle de mise en page commun aux écrans UI Toolkit (critères de sortie 2A) : un contrôle visible tient dans le panneau
    /// (hors liste défilante), garde une hauteur de manette (56 px de référence), ne recouvre aucun autre contrôle, aucun texte n'est écrasé
    /// en colonne quasi nulle, aucun texte affiché ne contient « Aube tactique ». Renvoie la liste des défauts (vide = conforme).
    /// </summary>
    internal static class UiLayoutChecker
    {
        public const float MinHeight = 56f;
        const float EdgeTolerance = 1f;

        static bool IsShown(VisualElement e)
        {
            for (var p = e; p != null; p = p.parent)
                if (p.resolvedStyle.display == DisplayStyle.None || p.resolvedStyle.opacity < 0.01f || p.resolvedStyle.visibility == Visibility.Hidden) return false;
            return true;
        }

        static string Name(VisualElement e)
        {
            if (!string.IsNullOrEmpty(e.name)) return e.name;
            if (e is Button b && !string.IsNullOrEmpty(b.text)) return b.text;
            var classes = string.Join(".", e.GetClasses().Take(2));
            return classes.Length > 0 ? classes : e.GetType().Name;
        }

        static bool IsControl(VisualElement e) => e is Button || e is TextField || e is Toggle || e is Slider || e is SliderInt || e is DropdownField;

        /// <summary>Contrôles visibles et posés (largeur et hauteur non nulles).</summary>
        public static List<VisualElement> Controls(VisualElement root)
            => root.Query<VisualElement>().ToList().Where(e => IsControl(e) && e.worldBound.width > 1 && e.worldBound.height > 1 && IsShown(e)).ToList();

        /// <summary>Zone d appui qui couvre au moins la moitié du panneau : un fond cliquable, pas un bouton à poser.</summary>
        static bool IsBackdrop(VisualElement e, Rect panel) => e.worldBound.width * e.worldBound.height >= 0.5f * panel.width * panel.height;

        public static List<string> Analyze(VisualElement root, string tag)
        {
            var problems = new List<string>();
            if (root.panel == null) { problems.Add(tag + " : racine hors panneau"); return problems; }
            var panel = root.panel.visualTree.worldBound;
            var controls = Controls(root);
            foreach (var c in controls)
            {
                var wb = c.worldBound;
                bool scrolled = c.GetFirstAncestorOfType<ScrollView>() != null;
                if (!scrolled && (wb.xMin < panel.xMin - EdgeTolerance || wb.xMax > panel.xMax + EdgeTolerance || wb.yMin < panel.yMin - EdgeTolerance || wb.yMax > panel.yMax + EdgeTolerance))
                    problems.Add($"{tag} : « {Name(c)} » déborde du panneau ({wb} dans {panel})");
                if ((c is Button || c is TextField) && wb.height < MinHeight - 0.5f)
                    problems.Add($"{tag} : « {Name(c)} » sous {MinHeight:F0} px de référence ({wb.height:F0})");
            }
            for (int i = 0; i < controls.Count; i++)
                for (int j = i + 1; j < controls.Count; j++)
                {
                    var a = controls[i]; var b = controls[j];
                    if (a.Contains(b) || b.Contains(a)) continue; // un champ contient son entrée de texte
                    if (IsBackdrop(a, panel) || IsBackdrop(b, panel)) continue; // la zone d appui plein écran (passer, suite) est sous les boutons par construction
                    if ((a.GetFirstAncestorOfType<ScrollView>() == null) != (b.GetFirstAncestorOfType<ScrollView>() == null)) continue; // une liste défile sous le reste
                    var ra = a.worldBound; var rb = b.worldBound;
                    float w = Mathf.Min(ra.xMax, rb.xMax) - Mathf.Max(ra.xMin, rb.xMin), h = Mathf.Min(ra.yMax, rb.yMax) - Mathf.Max(ra.yMin, rb.yMin);
                    if (w > 4f && h > 4f) problems.Add($"{tag} : « {Name(a)} » et « {Name(b)} » se chevauchent ({w:F0}×{h:F0} px)");
                }
            foreach (var l in root.Query<Label>().ToList())
            {
                if (string.IsNullOrEmpty(l.text) || l.worldBound.height <= 1 || !IsShown(l)) continue;
                if (l.text.Length >= 8 && l.worldBound.width < 40f)
                    problems.Add($"{tag} : le texte « {l.text.Substring(0, Mathf.Min(24, l.text.Length))} » est écrasé ({l.worldBound.width:F0} px de large)");
                if (l.text.IndexOf("aube tactique", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    problems.Add($"{tag} : le texte affiché contient « Aube tactique » (« {l.text} »)");
            }
            foreach (var b in controls.OfType<Button>())
                if (b.text != null && b.text.IndexOf("aube tactique", System.StringComparison.OrdinalIgnoreCase) >= 0)
                    problems.Add($"{tag} : le bouton « {b.text} » contient « Aube tactique »");
            return problems.Distinct().ToList();
        }
    }
}
