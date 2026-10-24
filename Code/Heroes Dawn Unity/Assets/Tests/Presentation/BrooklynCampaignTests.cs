using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Campagne de Brooklyn (spec 2026-10-12) : quatre zones, objectifs, ennemis et textes cohérents avec l'histoire.</summary>
    public sealed class BrooklynCampaignTests
    {
        static readonly ContentCatalog C = PrototypeContent.Build();
        static readonly LocLocalization L = new LocLocalization();

        [Test, Description("Vérifie que Brooklyn compte quatre zones 1.1 à 1.4 dans l'ordre de la campagne, déblocage linéaire — parce que c'est la campagne fournie par l'équipe")]
        public void FourZones_InOrder_Linear()
        {
            var zones = C.Zones.Where(z => z.Borough == "brooklyn").OrderBy(z => z.Code).ToList();
            Assert.That(zones.Select(z => z.Code), Is.EqualTo(new[] { "1.1", "1.2", "1.3", "1.4" }));
            Assert.That(zones[0].Requires, Is.Empty);
            for (int i = 1; i < zones.Count; i++) Assert.That(zones[i].Requires.Select(r => r.Value), Is.EqualTo(new[] { zones[i - 1].Id.Value }), zones[i].Code);
        }

        [Test, Description("Vérifie que la zone 1.1 reprend exactement les cinq objectifs de l'équipe (infiltrer, courant, documents, échange, fuite) dans les bons types — parce que ces objectifs viennent du game design")]
        public void RedHook_HasTheFiveObjectivesOfTheDesign()
        {
            var o = C.Zone(PrototypeContent.Zone11).Objectives;
            Assert.That(o.Select(x => x.Type), Is.EqualTo(new[]
            {
                ObjectiveType.ReachCase, ObjectiveType.CompleteMiniGame, ObjectiveType.ReachCase, ObjectiveType.DefeatCombat, ObjectiveType.ReachCase,
            }));
            Assert.That(o.Select(x => L.T(x.NameKey)), Is.EqualTo(new[]
            {
                "Franchir les grilles du quai", "Plonger le port dans le noir (générateur)", "Arracher les registres de transport", "Briser l'échange en cours", "Disparaître avec la vérité",
            }));
        }

        [Test, Description("Vérifie que les zones 1.3 et 1.4 se terminent sur un boss (Crocodile, puis le Titan de l'AMIF) et que 1.2 a son lieutenant d'élite — parce que l'histoire a un climax par zone")]
        public void Climaxes_AreCrocodileAndTheAmifTitan()
        {
            var z12 = C.Zone(PrototypeZones.Zone12);
            var z13 = C.Zone(PrototypeZones.Zone13);
            var z14 = C.Zone(PrototypeZones.Zone14);
            Assert.That(z12.Nodes.Any(n => n.Type == CaseType.Combat && n.Rank == CombatRank.Elite), Is.True);
            Assert.That(z13.Encounters.Single(e => e.Rank == CombatRank.Boss).Enemies.Select(e => e.Value), Does.Contain("crocodile"));
            Assert.That(z14.Encounters.Single(e => e.Rank == CombatRank.Boss).Enemies.Select(e => e.Value), Does.Contain("bridge_titan"));
            Assert.That(C.Enemy(new EnemyId("crocodile")).IsBoss, Is.True);
            Assert.That(z14.Encounters.SelectMany(e => e.Enemies).Select(e => e.Value), Does.Contain("amif_trooper"), "la zone 1.4 affronte l'AMIF");
        }

        [Test, Description("Vérifie que chaque nom, description, objectif, réplique de narration, ennemi et compétence ennemie des quatre zones a un texte français — parce qu'une clé manquante s'afficherait telle quelle à l'écran")]
        public void EveryTextOfTheCampaign_Exists()
        {
            var missing = new System.Collections.Generic.List<string>();
            void Need(string key) { if (!L.Has(key)) missing.Add(key); }
            foreach (var z in C.Zones.Where(z => z.Borough == "brooklyn"))
            {
                Need(z.NameKey);
                Need(z.NameKey.Replace(".name", ".short"));
                Need(z.NameKey.Replace(".name", ".desc"));
                Need("qg.fil." + z.Id.Value);
                Need("archives.zone." + z.Id.Value);
                foreach (var o in z.Objectives) Need(o.NameKey);
                foreach (var n in z.Nodes.Where(n => n.Narration != null))
                    foreach (var line in n.Narration!.Before.Concat(n.Narration.After)) Need(line.TextKey);
                foreach (var id in z.Encounters.SelectMany(e => e.Enemies).Distinct())
                {
                    var e = C.Enemy(id);
                    Need("enemies." + id.Value + ".name");
                    foreach (var s in e.Skills) Need("skills." + s.Id.Value);
                }
            }
            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Test, Description("Vérifie que chaque personnage qui parle dans la campagne (hors agents) a un nom affichable — parce que la boîte de dialogue montre le nom du locuteur")]
        public void EverySpeaker_HasADisplayName()
        {
            var members = C.Members.Select(m => m.Id.Value).ToHashSet();
            var missing = C.Zones.Where(z => z.Borough == "brooklyn").SelectMany(z => z.Nodes)
                .Where(n => n.Narration != null).SelectMany(n => n.Narration!.Before.Concat(n.Narration.After))
                .Select(l => l.Speaker).Distinct().Where(s => !members.Contains(s) && !L.Has("speaker." + s)).ToList();
            Assert.That(missing, Is.Empty, string.Join(", ", missing));
        }
    }
}
