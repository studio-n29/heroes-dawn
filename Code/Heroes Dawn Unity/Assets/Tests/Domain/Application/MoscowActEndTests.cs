#nullable enable
using System.Linq;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain.Application
{
    /// <summary>Fin de l'acte I d'Agent M (le Passeur vaincu) : Paris sans zone, plus aucune zone ouverte ni proposée ; Agent D inchangé après Brooklyn.</summary>
    public sealed class MoscowActEndTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static GameSession NewSession(string hero)
        {
            var s = new GameSession(C);
            s.ResetToNewProfile();
            s.Meta.onboarding.hero = hero;
            return s;
        }

        static void CompleteMoscow(GameSession s) { foreach (var z in StoryProgress.ZonesOf(s, "moscow")) s.CompletedZones.Add(z.Id.Value); }

        [Test, Description("Vérifie qu'un profil Agent M qui a validé M1.4 est à l'acte II (Paris, sans zone) — parce que le chapitre se termine sur la scène du Passeur et l'écran suivant est « acte suivant bientôt »")]
        public void AgentM_AfterM14_CurrentActIsParis_WithoutZones()
        {
            var s = NewSession("agent_m");
            Assert.That(StoryProgress.Current(s).City, Is.EqualTo("moscow"));
            Assert.That(StoryProgress.ZonesOf(s, "moscow").Select(z => z.Code), Is.EqualTo(new[] { "M1.1", "M1.2", "M1.3", "M1.4" }));
            CompleteMoscow(s);
            var act = StoryProgress.Current(s);
            Assert.That(act.Number, Is.EqualTo(2));
            Assert.That(act.City, Is.EqualTo("paris"));
            Assert.That(act.HasZones, Is.False);
        }

        [Test, Description("Vérifie qu'après le Passeur aucune zone n'est ouverte (ni Brooklyn, ni son repaire) et que la zone recommandée est une zone de Moscou validée — parce que l'écran ne doit proposer ni Brooklyn ni un plateau vide")]
        public void AgentM_AfterM14_NoZoneIsRecommendedOrOpen()
        {
            var s = NewSession("agent_m");
            CompleteMoscow(s);
            Assert.That(C.Zones.Where(z => !z.IsTutorial).Select(z => ZoneProgress.StatusOf(s, z)).Count(st => st == ZoneStatus.Available), Is.EqualTo(0));
            var rec = ZoneProgress.Recommended(s);
            Assert.That(rec.Borough, Is.EqualTo("sergueiv"));
            Assert.That(ZoneProgress.StatusOf(s, rec), Is.EqualTo(ZoneStatus.Done));
        }

        [Test, Description("Vérifie qu'un profil Agent D garde le même parcours après Brooklyn : l'acte II est Paris sans zone, Moscou reste fermée — parce que le comportement d'Agent D ne doit pas changer")]
        public void AgentD_Unchanged_AfterBrooklyn()
        {
            var s = NewSession("agent_d");
            foreach (var z in StoryProgress.ZonesOf(s, "new_york")) s.CompletedZones.Add(z.Id.Value); // y compris le repaire de l AMIF : ZonesOf le compte dans New York (comportement existant, voir rapport)
            var act = StoryProgress.Current(s);
            Assert.That(act.Number, Is.EqualTo(2));
            Assert.That(act.City, Is.EqualTo("paris"));
            Assert.That(act.HasZones, Is.False);
            Assert.That(StoryProgress.ZonesOf(s, "moscow").All(z => ZoneProgress.StatusOf(s, z) == ZoneStatus.Locked), Is.True);
            Assert.That(ZoneProgress.Recommended(s).Borough, Is.Not.EqualTo("sergueiv"));
        }

        [Test, Description("Vérifie que le journal de quêtes d'Agent M liste les quatre zones de Moscou (la première en cours) et aucune zone de Brooklyn — parce qu'un profil Agent M ne doit voir aucune zone de Brooklyn")]
        public void AgentM_QuestLog_ListsOnlyMoscow()
        {
            var app = new TestApp();
            app.SignInDirect("p1");
            app.Session.Meta.onboarding.hero = "agent_m";
            var main = app.Quests.Main();
            Assert.That(main.Select(m => m.Zone.Code), Is.EqualTo(new[] { "M1.1", "M1.2", "M1.3", "M1.4" }));
            Assert.That(main.Select(m => m.Status), Is.EqualTo(new[] { ZoneStatus.Available, ZoneStatus.Locked, ZoneStatus.Locked, ZoneStatus.Locked }));
            Assert.That(app.Quests.CurrentMain()!.Value.Zone.Code, Is.EqualTo("M1.1"));
        }
    }
}
