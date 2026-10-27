using System.Linq;
using N29.HeroesDawn.Application.Features.Combat;
using N29.HeroesDawn.Application.Features.Meta;
using N29.HeroesDawn.Application.Ports;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Tag, pseudo et statistiques affichés sur la page Profil.</summary>
    public sealed class PlayerProfileTests
    {
        TestApp _app = null!;

        [SetUp]
        public void SetUp()
        {
            _app = new TestApp();
            _app.SignInDirect("p1");
        }

        [Test, Description("Vérifie que le tag reprend le pseudo et l'identifiant du joueur, et que sans pseudo on affiche « Invité » — parce que le tag doit toujours être valide")]
        public void Tag_UsesPseudoAndPlayerId()
        {
            _app.Session.Meta.onboarding.pseudo = "Dorian";
            Assert.That(_app.Profile.Tag, Is.EqualTo("Dorian#1274"));
            Assert.That(_app.Profile.Pseudo, Is.EqualTo("Dorian"));
            _app.Session.Meta.onboarding.pseudo = "";
            Assert.That(_app.Profile.Tag, Is.EqualTo("Invité#1274"));
            Assert.That(_app.Profile.Pseudo, Is.EqualTo("Invité"));
        }

        [Test, Description("Vérifie que les statistiques lisent les compteurs de la sauvegarde et l'état de la session (zones, effectif, liens, objets fabriqués) — parce que la page Profil ne calcule rien d'autre")]
        public void Stats_ReadTheSaveAndTheSession()
        {
            var meta = _app.Session.Meta;
            Stats.Add(meta, Stats.CombatsWon, 2); Stats.Add(meta, Stats.CombatsLost, 1); Stats.Add(meta, Stats.CreditsEarned, 4200);
            foreach (var z in new[] { "z1", "z2", "z3" }) _app.Session.CompletedZones.Add(z);
            var ids = _app.Session.Roster.Select(m => m.Id.Value).ToList();
            for (int i = 0; i < 7; i++) TeamLinks.RecordVictory(meta, new[] { ids[0], ids[1] });
            meta.crafted.Add(new CraftedItemSave { id = "crafted_1" });
            meta.crafted.Add(new CraftedItemSave { id = "crafted_2" });

            var s = _app.Profile.Stats();
            Assert.That((s.CombatsWon, s.CombatsLost, s.CreditsEarned), Is.EqualTo((2, 1, 4200)));
            Assert.That(s.ZonesCleared, Is.EqualTo(3));
            Assert.That(s.Agents, Is.EqualTo(_app.Session.Roster.Count));
            Assert.That(s.TopAgentLevel, Is.EqualTo(_app.Session.Roster.Max(m => m.Level)));
            Assert.That(s.BestLinkWins, Is.EqualTo(7));
            Assert.That(s.CraftedItems, Is.EqualTo(2));
        }

        [Test, Description("Vérifie qu'une sauvegarde neuve donne zéro partout sauf l'effectif — parce qu'un nouveau joueur n'a encore rien accompli")]
        public void Stats_OfAnEmptySave_AreZero()
        {
            var s = _app.Profile.Stats();
            Assert.That((s.CombatsWon, s.CombatsLost, s.ZonesCleared, s.BestLinkWins, s.CraftedItems, s.CreditsEarned), Is.EqualTo((0, 0, 0, 0, 0, 0)));
            Assert.That(s.Agents, Is.GreaterThan(0));
        }
    }
}
