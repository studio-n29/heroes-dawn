using N29.HeroesDawn.Application.Features.Meta;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Réglage du formulaire Google Forms des retours de playtest.</summary>
    public sealed class PlaytestFormConfigTests
    {
        const string Text = "# commentaire\nformResponse=https://docs.google.com/forms/d/e/ABC123/formResponse\nfun=entry.111\nclarity=entry.222\ndifficulty=entry.333\ntutorial=entry.444\ncomment=entry.555\nversion=\n";

        [Test, Description("Vérifie qu'un fichier de réglage complet est lu et active l'envoi — parce que l'adresse formResponse suffit à brancher le formulaire")]
        public void Parse_ReadsUrlAndEntries()
        {
            var c = PlaytestFormConfig.Parse(Text);
            Assert.That(c.Enabled, Is.True);
            var fields = c.Fields(new PlaytestAnswers(new[] { 5, 4, 3, 2 }, "super"), "0.12.0");
            Assert.That(fields.Count, Is.EqualTo(5), "quatre notes + commentaire ; la version n'a pas de champ");
            Assert.That(fields[0].Key, Is.EqualTo("entry.111"));
            Assert.That(fields[0].Value, Is.EqualTo("5"));
            Assert.That(fields[4].Value, Is.EqualTo("super"));
        }

        [Test, Description("Vérifie qu'un réglage vide ou une adresse qui n'est pas un formulaire Google n'active rien — parce qu'on n'envoie jamais de données vers une adresse inconnue")]
        public void EmptyOrForeignUrl_IsDisabled()
        {
            Assert.That(PlaytestFormConfig.Parse("formResponse=\nfun=entry.1").Enabled, Is.False);
            Assert.That(PlaytestFormConfig.Parse("formResponse=https://exemple.test/formResponse").Enabled, Is.False);
            Assert.That(PlaytestFormConfig.Parse("").Enabled, Is.False);
        }
    }
}
