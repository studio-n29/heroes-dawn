#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

namespace N29.HeroesDawn.Tests.PlayMode
{
    /// <summary>Contrôles communs des critères de sortie 2A sur le jeu démarré : quatre formats, cibles de manette, chemin de focus complet.</summary>
    internal static class UiToolkitAcceptance
    {
        internal static readonly (int w, int h, string name)[] Formats = { (1920, 1080, "16x9"), (2160, 972, "20x9"), (1280, 960, "4x3"), (1280, 720, "720p") };

        static bool IsShown(VisualElement e)
        {
            for (var p = e; p != null; p = p.parent) if (p.resolvedStyle.display == DisplayStyle.None || p.resolvedStyle.opacity < 0.01f) return false;
            return true;
        }

        /// <summary>Boutons visibles, actifs et cliquables de tous les documents actifs.</summary>
        internal static List<UnityEngine.UIElements.Button> VisibleButtons()
        {
            var list = new List<UnityEngine.UIElements.Button>();
            foreach (var d in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None).Where(d => d.isActiveAndEnabled && d.rootVisualElement != null))
                foreach (var b in d.rootVisualElement.Query<UnityEngine.UIElements.Button>().ToList())
                    if (b.resolvedStyle.display != DisplayStyle.None && b.resolvedStyle.visibility == Visibility.Visible && b.worldBound.width > 1 && b.worldBound.height > 1
                        && b.enabledInHierarchy && b.pickingMode == PickingMode.Position && IsShown(b))
                        if (!list.Contains(b)) list.Add(b); // les documents d un même panneau sont imbriqués : un bouton peut être vu deux fois
            return list;
        }

        /// <summary>Éléments focalisables visibles (boutons, rangées, champs) : étapes possibles d'un parcours à la manette.</summary>
        internal static List<VisualElement> VisibleFocusables()
        {
            var list = new List<VisualElement>();
            foreach (var d in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None).Where(d => d.isActiveAndEnabled && d.rootVisualElement != null))
                foreach (var e in d.rootVisualElement.Query<VisualElement>().ToList())
                    if (e.focusable && e.canGrabFocus && e.worldBound.width > 1 && e.worldBound.height > 1 && IsShown(e) && !list.Contains(e)) list.Add(e);
            return list;
        }

        static string Path2(VisualElement e) { var s = ""; for (var p = e; p != null && s.Length < 160; p = p.parent) s += (string.IsNullOrEmpty(p.name) ? p.GetType().Name : p.name) + "<"; return s; }

        static string Label(VisualElement b) => string.IsNullOrEmpty(b.name) ? (b is UnityEngine.UIElements.Button bt ? bt.text : b.GetType().Name) : b.name;

        /// <summary>Rend les panneaux actifs aux quatre formats : boutons de <paramref name="scope"/> dans le panneau et d'au moins 56 px de référence.</summary>
        internal static IEnumerator CheckFormats(string folder, string screen, Func<UnityEngine.UIElements.Button, bool> scope, int minButtons, Action<string, List<string>>? extraChecks = null)
        {
            var panels = UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None).Where(d => d.isActiveAndEnabled && d.panelSettings != null).Select(d => d.panelSettings).Distinct().ToList();
            var saved = panels.Select(p => (p, p.targetTexture)).ToList();
            try
            {
                foreach (var (w, h, name) in Formats)
                {
                    var rts = new List<RenderTexture>();
                    foreach (var p in panels) { var rt = new RenderTexture(w, h, 24); rts.Add(rt); p.targetTexture = rt; }
                    for (int i = 0; i < 25; i++) yield return null;
                    var problems = new List<string>();
                    var buttons = VisibleButtons().Where(scope).ToList();
                    foreach (var b in buttons)
                    {
                        var panel = b.panel.visualTree.worldBound; var wb = b.worldBound;
                        // Les boutons d'une liste défilante (réglages, listes) sortent de l'écran par construction : la liste les fait défiler.
                        bool scrolled = b.GetFirstAncestorOfType<ScrollView>() != null;
                        if (!scrolled && (wb.xMin < panel.xMin - 1 || wb.xMax > panel.xMax + 1 || wb.yMin < panel.yMin - 1 || wb.yMax > panel.yMax + 1))
                            problems.Add($"« {Label(b)} » déborde du panneau ({wb} dans {panel})");
                        if (wb.height < 56f - 0.5f) problems.Add($"« {Label(b)} » sous 56 px de référence de hauteur ({wb.height:F0})");
                        // Un bouton coupé par un conteneur à débordement masqué (hors liste défilante) est inutilisable.
                        for (var p = b.parent; p != null; p = p.parent)
                        {
                            if (p is ScrollView) break;
                            bool clips = (p.ClassListContains("hd-list") || p.ClassListContains("hd-list__main") || p.ClassListContains("hd-window")) && p.worldBound.width > 1 // conteneurs à débordement masqué (USS)
                                         && (wb.xMin < p.worldBound.xMin - 1 || wb.xMax > p.worldBound.xMax + 1 || wb.yMin < p.worldBound.yMin - 1 || wb.yMax > p.worldBound.yMax + 1);
                            if (clips)
                            { problems.Add($"« {Label(b)} » est coupé par « {(string.IsNullOrEmpty(p.name) ? string.Join(".", p.GetClasses()) : p.name)} » ({wb} dans {p.worldBound})"); break; }
                        }
                    }
                    // Un texte écrasé dans une colonne presque nulle s'affiche lettre par lettre (constaté sur le pass en 4:3).
                    foreach (var d in UnityEngine.Object.FindObjectsByType<UIDocument>(FindObjectsSortMode.None).Where(d => d.isActiveAndEnabled && d.rootVisualElement != null))
                        foreach (var l in d.rootVisualElement.Query<Label>().ToList())
                            if (l.text != null && l.text.Length >= 8 && l.resolvedStyle.display != DisplayStyle.None && IsShown(l) && l.worldBound.height > 1 && l.worldBound.width < 40f)
                                problems.Add($"{name} : le texte « {l.text.Substring(0, Mathf.Min(24, l.text.Length))} » est écrasé ({l.worldBound.width:F0} px de large)");
                    extraChecks?.Invoke(name, problems);
                    Directory.CreateDirectory(folder);
                    // Un fichier par panneau (page, fenêtres et notifications sont des panneaux distincts).
                    for (int k = 0; k < rts.Count; k++)
                    {
                        var prev = RenderTexture.active; RenderTexture.active = rts[k];
                        var tex = new Texture2D(w, h, TextureFormat.RGB24, false); tex.ReadPixels(new Rect(0, 0, w, h), 0, 0); tex.Apply(); RenderTexture.active = prev;
                        File.WriteAllBytes(Path.Combine(folder, $"{screen}-{name}-p{k}.png"), tex.EncodeToPNG()); UnityEngine.Object.Destroy(tex);
                    }
                    foreach (var rt in rts) rt.Release();
                    Assert.That(buttons.Count, Is.GreaterThanOrEqualTo(minButtons), $"{screen} {name} : boutons exposés");
                    Assert.That(problems, Is.Empty, $"{screen} {name} ({buttons.Count} boutons) :\n" + string.Join("\n", problems.Distinct()));
                }
            }
            finally { foreach (var (p, t) in saved) p.targetTexture = t; }
        }

        /// <summary>Tous les boutons de <paramref name="scope"/> sont atteignables à la manette depuis le premier d'entre eux (parcours dans tout l'écran visible).</summary>
        internal static IEnumerator CheckGamepadReach(string screen, Func<UnityEngine.UIElements.Button, bool> scope, int minButtons)
        {
            var all = VisibleButtons();
            var target = all.Where(scope).ToList();
            Assert.That(target.Count, Is.GreaterThanOrEqualTo(minButtons), screen + " : boutons exposés");
            var edges = new Dictionary<VisualElement, HashSet<VisualElement>>();
            foreach (var b in VisibleFocusables())
            {
                edges[b] = new HashSet<VisualElement>();
                foreach (var dir in new[] { NavigationMoveEvent.Direction.Down, NavigationMoveEvent.Direction.Up, NavigationMoveEvent.Direction.Left, NavigationMoveEvent.Direction.Right, NavigationMoveEvent.Direction.Next, NavigationMoveEvent.Direction.Previous })
                {
                    b.Focus(); yield return null;
                    var fc = b.panel.focusController;
                    using (var e = NavigationMoveEvent.GetPooled(dir)) { e.target = b; b.SendEvent(e); }
                    yield return null;
                    if (fc.focusedElement is VisualElement f && f != b) edges[b].Add(f);
                }
            }
            // Un panneau superposé (tutoriel, fenêtre) a son propre focus et le piège : on part du premier bouton de chaque panneau.
            var reached = new HashSet<VisualElement>(); var queue = new Queue<VisualElement>();
            foreach (var start in target.GroupBy(b => b.panel).Select(g => g.First())) { reached.Add(start); queue.Enqueue(start); }
            while (queue.Count > 0)
            {
                var cur = queue.Dequeue();
                if (!edges.TryGetValue(cur, out var next)) continue;
                foreach (var n in next) if (reached.Add(n)) queue.Enqueue(n);
            }
            var unreachable = target.Where(b => !reached.Contains(b)).Select(b => Label(b) + " #" + b.GetHashCode() + " " + b.worldBound + " chemin " + Path2(b)).ToList();
            Assert.That(unreachable, Is.Empty, $"{screen} : boutons inaccessibles à la manette : {string.Join(", ", unreachable)} (sur {target.Count})");
        }
    }
}
