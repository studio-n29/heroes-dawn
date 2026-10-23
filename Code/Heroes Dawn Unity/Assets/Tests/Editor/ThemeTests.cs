using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;
using UnityEngine;

namespace N29.HeroesDawn.Tests.Editor
{
    public sealed class ThemeTests
    {
        [Test, Description("Vérifie que le texte courant atteint 4,5:1 sur les surfaces et le fond sombres, le néon 3:1 sur le verre et le grand texte 3:1 sur le corail — parce que le verre sombre translucide rend vite le texte moyen illisible")]
        public void Theme_TextOnSurfaces_MeetsContrast()
        {
            foreach (var bg in new[] { Theme.Surface, Theme.Surface2, Theme.Surface3, Theme.Background })
            {
                Assert.That(ThemeContrast.Ratio(Theme.Text, bg), Is.GreaterThanOrEqualTo(4.5f), "Text sur " + bg);
                Assert.That(ThemeContrast.Ratio(Theme.TextMuted, bg), Is.GreaterThanOrEqualTo(4.5f), "TextMuted sur " + bg);
            }
            Assert.That(ThemeContrast.Ratio(Theme.Neon, Theme.Surface), Is.GreaterThanOrEqualTo(3f), "liseré néon sur le verre");
            Assert.That(ThemeContrast.Ratio(Theme.TextOnDark, Theme.AccentDark), Is.GreaterThanOrEqualTo(3f), "grand texte clair sur corail soutenu");
            // Le texte des boutons corail clair (Accent) est sombre : le blanc n'y atteint pas 3:1.
            Assert.That(ThemeContrast.Ratio(Theme.TextDark, Theme.Accent), Is.GreaterThanOrEqualTo(4.5f), "texte nuit sur corail");
        }

        [Test, Description("Vérifie que les jetons du thème sombre ont les valeurs de la spec — parce que tout l'habillage en dépend")]
        public void Theme_DarkTokens_MatchSpec()
        {
            Assert.That(Theme.Background, Is.EqualTo(Theme.Hex(0x0B1020)));
            Assert.That(Theme.NightBottom, Is.EqualTo(Theme.Hex(0x1A1033)));
            Assert.That(Theme.Neon, Is.EqualTo(Theme.Hex(0x25E6FF)));
            Assert.That(Theme.Accent, Is.EqualTo(Theme.Hex(0xFF6B5E)));
            Assert.That(Theme.Text, Is.EqualTo(Theme.Hex(0xEAF1FF)));
            Assert.That(Theme.TextMuted, Is.EqualTo(Theme.Hex(0x9AA8D4)));
            Assert.That(Theme.FrostEdge, Is.EqualTo(Theme.NeonEdge));
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
