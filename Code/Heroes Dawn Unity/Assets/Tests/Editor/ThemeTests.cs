using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class ThemeTests
    {
        [Test, Description("Vérifie que le texte courant atteint 4,5:1 sur les surfaces et le fond du thème, et le grand texte blanc 3:1 sur le corail — parce que le pastel clair rend vite le texte illisible")]
        public void Theme_TextOnSurfaces_MeetsContrast()
        {
            foreach (var bg in new[] { Theme.Surface, Theme.Surface2, Theme.Surface3, Theme.Background })
                Assert.That(ThemeContrast.Ratio(Theme.Text, bg), Is.GreaterThanOrEqualTo(4.5f), "Text sur " + bg);
            foreach (var bg in new[] { Theme.Surface, Theme.Surface2, Theme.Surface3 })
                Assert.That(ThemeContrast.Ratio(Theme.TextMuted, bg), Is.GreaterThanOrEqualTo(4.5f), "TextMuted sur " + bg);
            Assert.That(ThemeContrast.Ratio(Theme.TextOnDark, Theme.Navy), Is.GreaterThanOrEqualTo(4.5f));
            Assert.That(ThemeContrast.Ratio(Theme.TextOnDark, Theme.AccentDark), Is.GreaterThanOrEqualTo(3f), "grand texte blanc sur corail soutenu");
            Assert.That(ThemeContrast.Ratio(Theme.Navy, Theme.Accent), Is.GreaterThanOrEqualTo(4.5f), "texte marine sur corail");
        }

        [Test, Description("Vérifie que les quatre familles restent deux à deux distinctes après l'adoucissement — parce qu'elles servent à lire le gameplay d'un coup d'œil")]
        public void Theme_FamiliesStayDistinct()
        {
            var fam = new[] { Family.Matter, Family.Elemental, Family.PhysicalBio, Family.Perception };
            for (int i = 0; i < fam.Length; i++)
            for (int j = i + 1; j < fam.Length; j++)
                Assert.That(Distance(Theme.FamilyColor(fam[i]), Theme.FamilyColor(fam[j])), Is.GreaterThanOrEqualTo(0.2f), fam[i] + "/" + fam[j]);
        }

        [Test, Description("Vérifie que les trois raretés restent distinctes — parce que le joueur doit les reconnaître sans lire")]
        public void Theme_RarityStaysDistinct()
        {
            var r = new[] { Rarity.Legend, Rarity.Elite, Rarity.Agent };
            for (int i = 0; i < r.Length; i++)
            for (int j = i + 1; j < r.Length; j++)
                Assert.That(Distance(Theme.RarityColor(r[i]), Theme.RarityColor(r[j])), Is.GreaterThanOrEqualTo(0.15f), r[i] + "/" + r[j]);
        }

        static float Distance(Color a, Color b) => Mathf.Sqrt((a.r - b.r) * (a.r - b.r) + (a.g - b.g) * (a.g - b.g) + (a.b - b.b) * (a.b - b.b));
    }
}
