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
            var shadow = frame.GetComponent<Shadow>();
            Assert.That(shadow.effectColor.a, Is.LessThanOrEqualTo(0.3f), "ombre douce");
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

        [Test, Description("Vérifie qu'un bouton désactivé a un libellé à 55 % d'alpha — parce qu'en pastel un bouton grisé doit rester distinct d'un bouton actif")]
        public void Button_DisabledLooksDisabled()
        {
            var b = Ui.Btn(_root.transform, "OK", Theme.Accent, Theme.TextOnDark, null);
            b.Interactable = false;
            Assert.That(b.Label.color.a, Is.EqualTo(0.55f).Within(0.001f));
            b.Interactable = true;
            Assert.That(b.Label.color.a, Is.EqualTo(1f));
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
    }
}
