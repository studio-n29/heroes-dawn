using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Editor
{
    /// <summary>
    /// Bug v1.0.0 : « Passer le tutoriel » (bouton ghost sombre) avait un texte bleu nuit (--hd-ink) donc invisible, et un bouton trop grand.
    /// Le texte doit être clair (ivoire / muted) et le bouton compact.
    /// </summary>
    public sealed class TutorialSkipButtonStyleTests
    {
        const string Shell = "Assets/_Project/UIToolkit/Theme/hd-shell.uss";

        static string Rule(string selector)
        {
            var m = Regex.Match(File.ReadAllText(Shell), Regex.Escape(selector) + @"\s*\{([^}]*)\}");
            Assert.IsTrue(m.Success, "Règle introuvable : " + selector);
            return m.Groups[1].Value;
        }

        [Test, Description("Vérifie que le texte du bouton « Passer le tutoriel » est ivoire et non bleu nuit (--hd-ink) — parce qu'un texte sombre sur le fond ghost sombre était invisible en v1.0.0")]
        public void SkipText_IsLightOnDarkGhostButton()
        {
            var body = Rule(".hd-tut__skip");
            var color = Regex.Match(body, @"(?<![-\w])color:\s*([^;]+);");
            Assert.IsTrue(color.Success, "color manquant");
            StringAssert.DoesNotContain("--hd-ink", color.Groups[1].Value, "texte sombre sur fond ghost sombre = invisible");
            StringAssert.Contains("--hd-ivory", color.Groups[1].Value);
        }

        [Test, Description("Vérifie que le chevron du bouton « Passer le tutoriel » n'est pas teinté en bleu nuit — parce qu'il serait invisible sur le fond ghost sombre")]
        public void SkipChevron_IsNotDark()
        {
            StringAssert.DoesNotContain("--hd-ink", Rule(".hd-tut__skip-chevron"));
        }

        [Test, Description("Vérifie que le bouton « Passer le tutoriel » est compact (56 à 60 px, police 22 à 26 px) mais reste une cible tactile lisible — parce qu'il doit rester plus discret que « Compris »")]
        public void SkipButton_IsCompactButTappable()
        {
            var body = Rule(".hd-tut__skip");
            int h = int.Parse(Regex.Match(body, @"min-height:\s*(\d+)px").Groups[1].Value);
            int fs = int.Parse(Regex.Match(body, @"font-size:\s*(\d+)px").Groups[1].Value);
            Assert.GreaterOrEqual(h, 56, "cible tactile minimale");
            Assert.LessOrEqual(h, 60, "plus petit que COMPRIS (88px)");
            Assert.LessOrEqual(fs, 26);
            Assert.GreaterOrEqual(fs, 22, "lisible");
        }
    }
}
