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

        [Test, Description("Vérifie que Ui.Panel garde son contrat (cadre parent, enfant Fill étiré avec marge) et dessine un contour sombre épais autour d'un aplat plein — parce que tous les écrans l'appellent et que le style Mo.Co repose sur ce contour")]
        public void Panel_KeepsContract_AndHasFineOutline()
        {
            KitArt.Enabled = false; // contrat des formes générées ; l'habillage du kit a son propre test
            Image fill;
            try { fill = Ui.Panel(_root.transform, "P", Theme.Surface, 4f, 8f); }
            finally { KitArt.Enabled = true; }
            var frame = fill.Frame().GetComponent<Image>();
            Assert.That(fill.name, Is.EqualTo("Fill"));
            Assert.That(fill.rectTransform.offsetMin.x, Is.GreaterThan(0f), "marge intérieure");
            Assert.That(frame.color, Is.EqualTo(Theme.FrostEdge));
            Assert.That(fill.rectTransform.offsetMin.x, Is.LessThanOrEqualTo(2f), "filet fin Aube tactique");
            Assert.That(frame.sprite, Is.SameAs(Sprites.Panel));
            var shadow = frame.GetComponent<Shadow>();
            Assert.That(shadow.effectColor.a, Is.LessThanOrEqualTo(0.5f), "une seule ombre, jamais dure");
        }

        [Test, Description("Vérifie que l'habillage du kit Aube tactique s'applique aux panneaux et aux boutons fabriqués dans l'éditeur — parce que le jeu doit utiliser les surfaces et boutons du kit")]
        public void KitSkin_IsAppliedToPanelsAndButtons()
        {
            Assert.That(KitArt.Get("Surfaces/panel-navy"), Is.Not.Null, "surface du kit importée");
            var fill = Ui.Panel(_root.transform, "Kit", Theme.Surface, 4f, 8f);
            Assert.That(fill.sprite, Is.SameAs(KitArt.Get("Surfaces/panel-navy")), "fond sombre : panel-navy");
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

    }
}
