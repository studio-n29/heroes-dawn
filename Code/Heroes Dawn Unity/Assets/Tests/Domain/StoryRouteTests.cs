using System.Linq;
using N29.HeroesDawn.Application.Features.World;
using N29.HeroesDawn.Application.Session;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Choix du personnage joueur (Agent D ou Agent M) et parcours en quatre actes (morceau 3).</summary>
    public sealed class StoryRouteTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();

        static GameSession NewSession(string hero = "")
        {
            var s = new GameSession(C);
            s.ResetToNewProfile();
            s.Meta.onboarding.hero = hero;
            return s;
        }

        [Test, Description("Vérifie l'ordre des actes des deux parcours : D va New York, Paris, Moscou, Salem et M va Moscou, Paris, New York, Salem — parce que c'est le fil de l'histoire écrit dans le lore")]
        public void Routes_FollowTheLoreOrder()
        {
            Assert.That(StoryRoutes.D.ActCities, Is.EqualTo(new[] { "new_york", "paris", "moscow", "salem" }));
            Assert.That(StoryRoutes.M.ActCities, Is.EqualTo(new[] { "moscow", "paris", "new_york", "salem" }));
            Assert.That(StoryRoutes.All.All(r => r.ActCities.Last() == StoryRoutes.Salem), Is.True, "Salem conclut chaque parcours");
        }

        [Test, Description("Vérifie qu'un profil sans choix suit le parcours d'Agent D — parce que les profils existants et les tests sans première connexion doivent rester jouables")]
        public void NoChoice_DefaultsToAgentD()
        {
            Assert.That(NewSession().Route, Is.SameAs(StoryRoutes.D));
            Assert.That(NewSession("agent_m").Route, Is.SameAs(StoryRoutes.M));
            Assert.That(StoryRoutes.For("inconnu"), Is.SameAs(StoryRoutes.D));
        }

        [Test, Description("Vérifie que les deux personnages joueurs existent, ne sont jamais en boutique et que l'équipe de départ de chaque parcours est composée de quatre agents du catalogue dont son personnage — parce qu'un agent manquant laisserait l'équipe vide")]
        public void Heroes_AreInTheRosterButNeverInTheShop()
        {
            foreach (var route in StoryRoutes.All)
            {
                var hero = C.Member(new MemberId(route.HeroId));
                Assert.That(hero.Unlock!.Kind, Is.EqualTo(UnlockKind.Hero), route.Id);
                Assert.That(hero.InShop, Is.False, route.Id);
                Assert.That(route.StarterTeam.Count, Is.EqualTo(4), route.Id);
                Assert.That(route.StarterTeam[0], Is.EqualTo(route.HeroId));
                Assert.That(route.StarterTeam.All(id => C.Members.Any(m => m.Id.Value == id)), Is.True, route.Id);
            }
        }

        [Test, Description("Vérifie que choisir un parcours remplace l'effectif par son équipe de départ, tous en escouade, personnage joueur en tête — parce que l'agent choisi doit être dans l'équipe dès la première mission")]
        public void StartWithTeam_ReplacesTheRoster()
        {
            var s = NewSession();
            s.StartWithTeam(StoryRoutes.M.StarterTeam);
            Assert.That(s.Roster.Select(m => m.Id.Value), Is.EqualTo(StoryRoutes.M.StarterTeam));
            Assert.That(s.Roster.All(m => m.InSquad), Is.True);
            Assert.That(s.Owns(new MemberId(StoryRoutes.AgentD)), Is.False, "un seul personnage joueur par partie");
        }

        [Test, Description("Vérifie que l'acte en cours est l'acte I au départ, et que l'acte II (Paris, sans zone créée) prend le relais quand toutes les zones de New York sont validées — parce que la carte et les quêtes suivent cet acte")]
        public void CurrentAct_FollowsTheCompletedCities()
        {
            var s = NewSession();
            var act = StoryProgress.Current(s);
            Assert.That(act.Number, Is.EqualTo(1));
            Assert.That(act.City, Is.EqualTo("new_york"));
            Assert.That(act.HasZones, Is.True);

            foreach (var z in StoryProgress.ZonesOf(s, "new_york")) s.CompletedZones.Add(z.Id.Value);
            act = StoryProgress.Current(s);
            Assert.That(act.Number, Is.EqualTo(2));
            Assert.That(act.City, Is.EqualTo("paris"));
            Assert.That(act.HasZones, Is.False, "les zones de Paris ne sont pas encore créées");
        }

        [Test, Description("Vérifie que le parcours d'Agent M commence à Moscou (sans zone créée) puis passe à Paris — parce que l'ordre des actes dépend du personnage")]
        public void CurrentAct_OfAgentM_StartsInMoscow()
        {
            var s = NewSession("agent_m");
            var act = StoryProgress.Current(s);
            Assert.That(act.Number, Is.EqualTo(1));
            Assert.That(act.City, Is.EqualTo("moscow"));
            Assert.That(act.HasZones, Is.False);
        }
    }
}
