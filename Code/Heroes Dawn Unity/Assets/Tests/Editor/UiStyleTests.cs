#nullable enable
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class UiStyleTests
    {
        GameObject _root = null!;

        [SetUp] public void SetUp() => _root = new GameObject("root", typeof(RectTransform));
        [TearDown] public void TearDown() => Object.DestroyImmediate(_root);

        [Test, Description("Vérifie que Ui.Panel garde son contrat (cadre parent, enfant Fill étiré avec marge) et dessine un filet clair sans contour noir ni ombre dure — parce que tous les écrans l'appellent et que le HUD pastel proscrit les gros contours noirs")]
        public void Panel_KeepsContract_AndHasNoBlackOutline()
        {
            var fill = Ui.Panel(_root.transform, "P", Theme.Surface, 4f, 8f);
            var frame = fill.Frame().GetComponent<Image>();
            Assert.That(fill.name, Is.EqualTo("Fill"));
            Assert.That(fill.rectTransform.offsetMin.x, Is.GreaterThan(0f), "marge intérieure");
            Assert.That(frame.color, Is.Not.EqualTo(Color.black));
            Assert.That(frame.color, Is.EqualTo(Theme.FrostEdge));
            Assert.That(frame.sprite, Is.SameAs(Sprites.Panel));
            var shadows = frame.GetComponents<Shadow>();
            Assert.That(shadows.Length, Is.EqualTo(3), "ombre en trois couches");
            foreach (var s in shadows) Assert.That(s.effectColor.a, Is.LessThanOrEqualTo(0.5f), "ombre douce");
            Assert.That(shadows[0].effectDistance.magnitude, Is.LessThan(shadows[2].effectDistance.magnitude), "contact puis diffuse");
        }

        [Test, Description("Vérifie que le panneau de verre porte un reflet haut qui reste dans sa moitié supérieure et un reflet diagonal — parce qu'un reflet qui déborde du cadre casserait les petits panneaux (barres, jauges)")]
        public void Panel_HasSheenInsideFrame()
        {
            var fill = Ui.Panel(_root.transform, "P", Theme.Surface);
            var sheen = (RectTransform)fill.transform.Find("Sheen");
            Assert.That(sheen, Is.Not.Null);
            Assert.That(sheen.anchorMax.y, Is.EqualTo(1f));
            Assert.That(sheen.anchorMin.y, Is.GreaterThanOrEqualTo(0.5f));
            Assert.That(sheen.GetComponent<Image>().raycastTarget, Is.False);
            Assert.That(fill.transform.Find("Reflect"), Is.Not.Null, "reflet diagonal");
        }

        [Test, Description("Vérifie qu'un panneau sans ombre n'a aucun Shadow ni halo — parce que les éléments à plat (listes denses) ne doivent pas payer le coût du relief")]
        public void Panel_WithoutShadow_HasNoShadowNorHalo()
        {
            var fill = Ui.Panel(_root.transform, "P", Theme.Surface, 4f, 0f);
            Assert.That(fill.Frame().GetComponents<Shadow>().Length, Is.EqualTo(0));
            Assert.That(fill.Frame().Find("Halo"), Is.Null);
        }

        [Test, Description("Vérifie que la variante Active a un liseré néon plein et un halo derrière le remplissage, et la variante Accent un liseré corail — parce que l'état actif doit se lire d'un coup d'œil")]
        public void Panel_Variants_ChangeEdgeAndHalo()
        {
            var active = Ui.Panel(_root.transform, "A", Theme.Surface, 4f, 8f, true, PanelVariant.Active);
            Assert.That(active.Frame().GetComponent<Image>().color, Is.EqualTo(Theme.Neon));
            var halo = active.Frame().Find("Halo");
            Assert.That(halo, Is.Not.Null);
            Assert.That(halo.GetSiblingIndex(), Is.LessThan(active.transform.GetSiblingIndex()), "halo derrière le remplissage");
            var accent = Ui.Panel(_root.transform, "C", Theme.Surface, 4f, 8f, true, PanelVariant.Accent);
            Assert.That(accent.Frame().GetComponent<Image>().color, Is.EqualTo(Theme.Accent));
            Assert.That(accent.Frame().Find("Halo"), Is.Null);
        }

        [Test, Description("Vérifie que la barre d'accent est collée au bord gauche et fait 4 px de large — parce que c'est la signature visuelle des panneaux importants")]
        public void AccentBar_IsLeftEdge()
        {
            var fill = Ui.Panel(_root.transform, "P", Theme.Surface);
            var bar = Ui.AccentBar(fill);
            Assert.That(bar.rectTransform.anchorMin.x, Is.EqualTo(0f));
            Assert.That(bar.rectTransform.anchorMax.x, Is.EqualTo(0f));
            Assert.That(bar.rectTransform.sizeDelta.x, Is.EqualTo(4f));
            Assert.That(bar.color, Is.EqualTo(Theme.Accent));
        }

        [Test, Description("Vérifie qu'un bouton désactivé a un fond assombri et un libellé estompé encore lisible — parce qu'en thème sombre un bouton grisé doit rester distinct d'un bouton actif sans devenir illisible")]
        public void Button_DisabledLooksDisabled()
        {
            var b = Ui.Btn(_root.transform, "OK", Theme.Accent, Theme.TextDark, null);
            b.Interactable = false;
            Assert.That(b.Fill.color, Is.Not.EqualTo(Theme.Accent), "le fond est éclairci");
            Assert.That(ThemeContrast.Ratio(b.Label.color, b.Fill.color), Is.GreaterThan(1.5f), "le libellé reste lisible sur le fond grisé");
            Assert.That(b.Label.color.a, Is.LessThan(1f), "libellé estompé");
            b.Interactable = true;
            Assert.That(b.Fill.color, Is.EqualTo(Theme.Accent));
            Assert.That(b.Label.color, Is.EqualTo(Theme.TextDark));
        }

        [Test, Description("Vérifie que la barre à 0 masque son remplissage et à 1 l'étire entièrement — parce que sans contour noir un remplissage résiduel à 0 se verrait")]
        public void Bar_ZeroAndFull()
        {
            var bar = Ui.Bar(_root.transform, "B", Theme.Hp, Theme.Surface2);
            bar.Set(0f, true);
            Assert.That(bar.transform.Find("Back/Fill").GetComponent<Image>().enabled, Is.False);
            bar.Set(1f, true);
            var fill = (RectTransform)bar.transform.Find("Back/Fill");
            Assert.That(fill.anchorMax.x, Is.EqualTo(1f));
            Assert.That(fill.GetComponent<Image>().enabled, Is.True);
        }

        [Test, Description("Vérifie que le remplissage de la barre a un glow qui disparaît à 0 et réapparaît à 1 — parce qu'un glow résiduel sur une barre vide ferait croire qu'il reste des PV")]
        public void Bar_GlowFollowsFill()
        {
            var bar = Ui.Bar(_root.transform, "B", Theme.Hp, Theme.Surface2);
            var glow = bar.transform.Find("Back/Fill/Glow").GetComponent<Image>();
            bar.Set(0f, true);
            Assert.That(glow.enabled, Is.False);
            bar.Set(1f, true);
            Assert.That(glow.enabled, Is.True);
        }

        [Test, Description("Vérifie que le bouton principal a un halo corail quand il est actif et aucun quand il est désactivé — parce que le halo signale l'action possible")]
        public void Button_PrimaryHasHalo_DisabledHasNone()
        {
            var b = Ui.Btn(_root.transform, "OK", Theme.Accent, Theme.TextDark, null);
            var halo = b.transform.Find("Halo");
            Assert.That(halo, Is.Not.Null);
            Assert.That(halo.gameObject.activeSelf, Is.True);
            b.Interactable = false;
            Assert.That(halo.gameObject.activeSelf, Is.False);
            b.Interactable = true;
            Assert.That(halo.gameObject.activeSelf, Is.True);
            var secondary = Ui.Btn(_root.transform, "Non", Theme.Surface2, Theme.Text, null);
            Assert.That(secondary.transform.Find("Halo"), Is.Null);
        }

        [Test, Description("Vérifie que le fond nuit est étiré sur tout le cadre, ne capte aucun toucher et porte un vignettage — parce qu'il remplace les ciels pastel sous tous les écrans et ne doit jamais gêner les boutons")]
        public void NightBackdrop_IsFullStretchAndIgnoresInput()
        {
            var bd = Ui.NightBackdrop(_root.transform);
            Assert.That(bd.rectTransform.anchorMin, Is.EqualTo(Vector2.zero));
            Assert.That(bd.rectTransform.anchorMax, Is.EqualTo(Vector2.one));
            Assert.That(bd.raycastTarget, Is.False);
            Assert.That(bd.color, Is.EqualTo(Theme.NightBottom));
            Assert.That(bd.transform.Find("Gradient"), Is.Not.Null);
            Assert.That(bd.transform.Find("Vignette"), Is.Not.Null);
        }
    }
}
