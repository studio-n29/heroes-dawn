#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Ciblage fin des compétences (<see cref="TargetPick"/>) : qui est visé, tirages consommés, compatibilité.</summary>
    public sealed class TargetPickTests
    {
        /// <summary>« verre » (unité 0) et « givre » (unité 1) alliés ; ennemis dès l'unité 2. Verre lance aussitôt une carte de test (coût 0, 1 dégât).</summary>
        static CombatSimulation Cast(TargetRule target, TargetPick pick, string[] enemies, out int[] targets,
                                     Action<CombatSimulation>? before = null, int preferred = -1, StatusType pickStatus = StatusType.None, ulong seed = 11)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var skill = new SkillDef(new SkillId("card_test"), SkillSlot.A3, 0, target, new[] { new EffectDef(EffectType.Damage, 1) }, 0, pick, pickStatus);
            var card = new SkillCardDef(new CardId("card_test"), "cards.card_test.name", Family.Matter, 1, skill);
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(new[] { card }).ToList(), col.Boosters));
            var allies = new[]
            {
                new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { new CardId("card_test") }),
                new AllyEntry(new MemberId("givre"), 5, 0),
            };
            var sim = new CombatSimulation(catalog, new CombatSetup(seed, CombatRank.Normal, allies,
                                                                    enemies.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray(), false));
            foreach (var u in sim.Units) u.Point = 0;
            before?.Invoke(sim);
            var verre = sim.Unit(0);
            verre.Point = verre.ActionPoint;
            verre.Energy = verre.MaxEnergy;
            sim.Submit(CombatCommand.UseSkill(verre.Id, 2, preferred));
            var events = sim.Step(1).ToList();
            targets = events.OfType<SkillUsedEvent>().First(e => e.Actor == 0).Targets;
            return sim;
        }

        static readonly string[] Three = { "brute", "brute", "brute" }; // unités 2, 3, 4

        static void SetPct(CombatUnit u, int pct) => u.Hp = u.MaxHp * pct / 100;

        [Test, Description("Vérifie que LowestHp vise l'ennemi au pourcentage de PV le plus bas, malgré la cible voulue — parce que le choix fin remplace celui du joueur")]
        public void LowestHp_PicksTheLowestPercentEnemy()
        {
            Cast(TargetRule.SingleEnemy, TargetPick.LowestHp, Three, out var t, s => { SetPct(s.Unit(2), 80); SetPct(s.Unit(3), 30); SetPct(s.Unit(4), 60); }, preferred: 2);
            Assert.That(t, Is.EqualTo(new[] { 3 }));
        }

        [Test, Description("Vérifie que LowestHp départage deux ennemis à égalité par le plus petit identifiant — parce que le résultat doit être déterministe")]
        public void LowestHp_TieGoesToTheLowestId()
        {
            Cast(TargetRule.SingleEnemy, TargetPick.LowestHp, Three, out var t, s => { SetPct(s.Unit(2), 90); SetPct(s.Unit(3), 40); SetPct(s.Unit(4), 40); });
            Assert.That(t, Is.EqualTo(new[] { 3 }));
        }

        [Test, Description("Vérifie que HighestHp vise l'ennemi au pourcentage de PV le plus haut — parce que c'est l'inverse de LowestHp")]
        public void HighestHp_PicksTheHighestPercentEnemy()
        {
            Cast(TargetRule.SingleEnemy, TargetPick.HighestHp, Three, out var t, s => { SetPct(s.Unit(2), 50); SetPct(s.Unit(3), 70); SetPct(s.Unit(4), 20); }, preferred: 4);
            Assert.That(t, Is.EqualTo(new[] { 3 }));
        }

        [Test, Description("Vérifie que HighestHp vise l'allié le plus en forme pour SingleAlly, lanceur compris — parce que « allié » inclut le lanceur")]
        public void HighestHp_OnAllies_IncludesTheCaster()
        {
            Cast(TargetRule.SingleAlly, TargetPick.HighestHp, new[] { "brute" }, out var t, s => SetPct(s.Unit(1), 50));
            Assert.That(t, Is.EqualTo(new[] { 0 }));
        }

        [Test, Description("Vérifie que StatusHolder vise l'ennemi porteur de l'altération, sinon le premier vivant — parce que sans porteur il faut tout de même une cible")]
        public void StatusHolder_PicksTheCarrier_ElseFirstAlive()
        {
            Cast(TargetRule.SingleEnemy, TargetPick.StatusHolder, Three, out var carried, s => s.Unit(4).Statuses.Add(new StatusInstance(StatusType.Burn, 3, 0)), pickStatus: StatusType.Burn);
            Assert.That(carried, Is.EqualTo(new[] { 4 }), "porteur");
            Cast(TargetRule.SingleEnemy, TargetPick.StatusHolder, Three, out var none, s => s.Unit(2).Hp = 0, pickStatus: StatusType.Burn);
            Assert.That(none, Is.EqualTo(new[] { 3 }), "aucun porteur : premier vivant");
        }

        [Test, Description("Vérifie que StatusHolder vise sans porteur l'allié aux PV les plus bas pour SingleAlly — parce que c'est le repli défini pour les alliés")]
        public void StatusHolder_OnAllies_FallsBackToLowestHp()
        {
            Cast(TargetRule.SingleAlly, TargetPick.StatusHolder, new[] { "brute" }, out var t, s => SetPct(s.Unit(1), 40), pickStatus: StatusType.Burn);
            Assert.That(t, Is.EqualTo(new[] { 1 }));
        }

        [Test, Description("Vérifie que NextToAct vise l'ennemi le plus proche de son point d'action, égalité au plus petit identifiant — parce que le rapport Point / ActionPoint décide")]
        public void NextToAct_PicksTheEnemyClosestToItsActionPoint()
        {
            Cast(TargetRule.SingleEnemy, TargetPick.NextToAct, Three, out var t, s => { s.Unit(2).Point = 10; s.Unit(3).Point = 40; s.Unit(4).Point = 25; }, preferred: 2);
            Assert.That(t, Is.EqualTo(new[] { 3 }), "le plus avancé");
            Cast(TargetRule.SingleEnemy, TargetPick.NextToAct, Three, out var tie, s => { s.Unit(2).Point = 10; s.Unit(3).Point = 30; s.Unit(4).Point = 30; });
            Assert.That(tie, Is.EqualTo(new[] { 3 }), "égalité : plus petit identifiant");
        }

        [Test, Description("Vérifie que NextToAct vise l'allié le plus avancé sur la frise pour SingleAlly — parce que la règle vaut aussi pour le camp du lanceur")]
        public void NextToAct_OnAllies()
        {
            Cast(TargetRule.SingleAlly, TargetPick.NextToAct, new[] { "brute" }, out var t, s => s.Unit(1).Point = s.Unit(1).ActionPoint / 2);
            Assert.That(t, Is.EqualTo(new[] { 1 }));
        }

        [Test, Description("Vérifie que NextToAct allié écarte le lanceur et les alliés déjà prêts (ils n'avancent plus), que sans autre allié la liste est vide sans tirage, et que pour les ennemis un ennemi prêt reste visé — parce que Advance et Gauge + ne font rien sur un allié prêt")]
        public void NextToAct_OnAllies_ExcludesTheCasterAndReadyAllies()
        {
            Cast(TargetRule.SingleAlly, TargetPick.NextToAct, new[] { "brute" }, out var t, s => s.Unit(1).Point = s.Unit(1).ActionPoint / 2);
            Assert.That(t, Is.EqualTo(new[] { 1 }), "un allié non prêt est visé");
            var reference = Cast(TargetRule.SingleAlly, TargetPick.NextToAct, new[] { "brute" }, out _, s => s.Unit(1).Hp = 0, seed: 5); // aucun candidat du tout : référence sans tirage
            var ready = Cast(TargetRule.SingleAlly, TargetPick.NextToAct, new[] { "brute" }, out var none, s => s.Unit(1).Point = s.Unit(1).ActionPoint, seed: 5);
            Assert.That(none, Is.Empty, "le seul autre allié est prêt : personne à viser (le lanceur est exclu)");
            Assert.That(ready.RngProbeForTests(), Is.EqualTo(reference.RngProbeForTests()), "aucun tirage");
            Cast(TargetRule.SingleAlly, TargetPick.NextToAct, new[] { "brute" }, out var alone, s => s.Unit(1).Hp = 0);
            Assert.That(alone, Is.Empty, "aucun autre allié vivant");
            Cast(TargetRule.SingleEnemy, TargetPick.NextToAct, Three, out var foe, s => { s.Unit(2).Point = s.Unit(2).ActionPoint; s.Unit(3).Point = 10; });
            Assert.That(foe, Is.EqualTo(new[] { 2 }), "un ennemi prêt reste le plus avancé");
        }

        /// <summary>Combat Auto : « verre » lance le Sillage de test (1A:prochain, coût 0) ; l'allié « givre » est présent ou non.</summary>
        static CombatSimulation AutoWithSlipstream(bool withAlly, out List<SkillUsedEvent> casts)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var skill = new SkillDef(new SkillId("card_test"), SkillSlot.A3, 0, TargetRule.SingleAlly, new[] { new EffectDef(EffectType.Advance, 30) }, 0, TargetPick.NextToAct);
            var card = new SkillCardDef(new CardId("card_test"), "cards.card_test.name", Family.Matter, 1, skill);
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(new[] { card }).ToList(), col.Boosters));
            var allies = new List<AllyEntry> { new AllyEntry(new MemberId("verre"), 5, 0, 0, null, new CardId?[] { new CardId("card_test") }) };
            if (withAlly) allies.Add(new AllyEntry(new MemberId("givre"), 5, 0));
            var sim = new CombatSimulation(catalog, new CombatSetup(7, CombatRank.Normal, allies.ToArray(), new[] { new EnemyEntry(new EnemyId("brute"), 5) }, true));
            sim.Unit(0).Energy = sim.Unit(0).MaxEnergy;
            for (int i = 0; i < 600 && !sim.IsOver; i++) sim.Step(1);
            casts = sim.Log.OfType<SkillUsedEvent>().Where(e => e.Actor == 0 && e.Skill.Value == "card_test").ToList();
            return sim;
        }

        [Test, Description("Vérifie qu'en Auto un lanceur seul n'utilise jamais une compétence 1A:prochain faute de cible, alors qu'avec un allié non prêt il la lance — parce que l'énergie et le tour ne se dépensent pas dans le vide")]
        public void Ai_DoesNotCastAPickWithNoCandidate()
        {
            AutoWithSlipstream(false, out var alone);
            Assert.That(alone, Is.Empty, "seul : aucune cible, la compétence n'est jamais choisie");
            AutoWithSlipstream(true, out var withAlly);
            Assert.That(withAlly, Is.Not.Empty, "avec un allié non prêt : elle est lancée");
            Assert.That(withAlly.All(e => e.Targets.Length == 1 && e.Targets[0] == 1));
        }

        [Test, Description("Vérifie que la Provocation d'un ennemi l'emporte sur un choix fin à cible unique — parce que Taunt prime sur tout ciblage unique")]
        public void Taunt_StillWinsOverAFineEnemyPick()
        {
            Cast(TargetRule.SingleEnemy, TargetPick.LowestHp, Three, out var t, s => { SetPct(s.Unit(2), 10); s.Unit(4).Statuses.Add(new StatusInstance(StatusType.Taunt, 3, 0)); });
            Assert.That(t, Is.EqualTo(new[] { 4 }));
        }

        [Test, Description("Vérifie que Random vise un seul ennemi vivant, identique à graine égale, et que la graine fait varier le choix — parce que les combats seedés restent déterministes")]
        public void Random_DrawsOnce_AndIsDeterministicForASeed()
        {
            var a = Cast(TargetRule.SingleEnemy, TargetPick.Random, Three, out var ta, seed: 5);
            var b = Cast(TargetRule.SingleEnemy, TargetPick.Random, Three, out var tb, seed: 5);
            Assert.That(ta, Is.EqualTo(tb));
            Assert.That(ta.Length, Is.EqualTo(1));
            Assert.That(ta[0], Is.InRange(2, 4));
            Assert.That(a.RngProbeForTests(), Is.EqualTo(b.RngProbeForTests()));
            var seen = new HashSet<int>();
            for (ulong seed = 1; seed <= 40; seed++) { Cast(TargetRule.SingleEnemy, TargetPick.Random, Three, out var t, seed: seed); seen.Add(t[0]); }
            Assert.That(seen.Count, Is.GreaterThan(1), "le tirage varie selon la graine");
            Cast(TargetRule.SingleEnemy, TargetPick.Random, Three, out var skipDead, s => { s.Unit(2).Hp = 0; s.Unit(3).Hp = 0; });
            Assert.That(skipDead, Is.EqualTo(new[] { 4 }), "seuls les vivants sont candidats");
        }

        /// <summary>Valeur de sonde du générateur après avoir consommé <paramref name="extraDraws"/> tirages de plus (chaque sonde fait avancer le générateur d'un tirage).</summary>
        static uint ProbeAfter(CombatSimulation sim, int extraDraws)
        {
            for (int i = 0; i < extraDraws; i++) sim.RngProbeForTests();
            return sim.RngProbeForTests();
        }

        /// <summary>Vérifie qu'un lancer Random a consommé exactement un tirage de plus qu'un lancer Default de même graine (ni 0, ni 2).</summary>
        static void AssertExactlyOneExtraDraw(string[] enemies, Action<CombatSimulation>? before = null, TargetPick pick = TargetPick.Random)
        {
            var reference = Cast(TargetRule.SingleEnemy, TargetPick.Default, enemies, out _, before, seed: 5);
            var picked = Cast(TargetRule.SingleEnemy, pick, enemies, out _, before, seed: 5);
            var p = picked.RngProbeForTests();
            Assert.That(p, Is.EqualTo(ProbeAfter(Cast(TargetRule.SingleEnemy, TargetPick.Default, enemies, out _, before, seed: 5), 1)), "exactement un tirage de plus");
            Assert.That(p, Is.Not.EqualTo(ProbeAfter(reference, 0)), "pas zéro tirage");
            Assert.That(p, Is.Not.EqualTo(ProbeAfter(Cast(TargetRule.SingleEnemy, TargetPick.Default, enemies, out _, before, seed: 5), 2)), "pas deux tirages");
        }

        [Test, Description("Vérifie que Random consomme exactement un tirage du générateur de plus qu'un lancer Default de même graine — parce que ni zéro ni deux tirages ne sont permis")]
        public void Random_ConsumesExactlyOneDraw() => AssertExactlyOneExtraDraw(Three);

        [Test, Description("Vérifie que Random consomme encore un tirage quand un seul ennemi est vivant — parce que le tirage unique est le choix défini, même sans alternative")]
        public void Random_WithOneCandidate_StillConsumesOneDraw() => AssertExactlyOneExtraDraw(new[] { "brute" });

        [Test, Description("Vérifie que Random sous Provocation ne consomme aucun tirage et vise le provocateur — parce que la Provocation répond avant tout tirage")]
        public void TauntPlusRandom_ConsumesNoDraw()
        {
            Action<CombatSimulation> taunt = s => s.Unit(4).Statuses.Add(new StatusInstance(StatusType.Taunt, 3, 0));
            var def = Cast(TargetRule.SingleEnemy, TargetPick.Default, Three, out _, taunt, seed: 5);
            var rnd = Cast(TargetRule.SingleEnemy, TargetPick.Random, Three, out var t, taunt, seed: 5);
            Assert.That(t, Is.EqualTo(new[] { 4 }));
            Assert.That(rnd.RngProbeForTests(), Is.EqualTo(def.RngProbeForTests()));
        }

        [Test, Description("Vérifie qu'un choix fin sans aucun ennemi vivant renvoie une liste vide sans consommer de tirage — parce qu'on ne tire jamais quand il n'y a rien à choisir")]
        public void NoCandidate_ReturnsEmpty_WithoutDrawing()
        {
            Action<CombatSimulation> dead = s => { s.Unit(2).Hp = 0; s.Unit(3).Hp = 0; s.Unit(4).Hp = 0; };
            uint reference = Cast(TargetRule.SingleEnemy, TargetPick.Default, Three, out _, dead, seed: 5).RngProbeForTests();
            foreach (var (rule, pick) in new[] { (TargetRule.SingleEnemy, TargetPick.Random), (TargetRule.SingleEnemy, TargetPick.LowestHp), (TargetRule.AllEnemies, TargetPick.TwoRandom) })
            {
                var sim = Cast(rule, pick, Three, out var t, dead, seed: 5);
                Assert.That(t, Is.Empty, pick.ToString());
                Assert.That(sim.RngProbeForTests(), Is.EqualTo(reference), pick + " : aucun tirage");
            }
        }

        [Test, Description("Vérifie que TwoRandom sur AllEnemies vise deux ennemis distincts vivants, un seul s'il est seul — parce que le tirage se fait sans remise")]
        public void TwoRandom_PicksTwoDistinctEnemies_OrOneWhenAlone()
        {
            for (ulong seed = 1; seed <= 30; seed++)
            {
                Cast(TargetRule.AllEnemies, TargetPick.TwoRandom, Three, out var t, seed: seed);
                Assert.That(t.Length, Is.EqualTo(2));
                Assert.That(t.Distinct().Count(), Is.EqualTo(2));
                Assert.That(t.All(x => x >= 2 && x <= 4));
            }
            Cast(TargetRule.AllEnemies, TargetPick.TwoRandom, new[] { "brute" }, out var alone);
            Assert.That(alone, Is.EqualTo(new[] { 2 }));
            Cast(TargetRule.AllEnemies, TargetPick.TwoRandom, Three, out var one, s => { s.Unit(2).Hp = 0; s.Unit(4).Hp = 0; });
            Assert.That(one, Is.EqualTo(new[] { 3 }), "un seul vivant");
        }

        [Test, Description("Vérifie que TwoRandom parmi deux ennemis les vise tous les deux — parce que deux tirages sans remise parmi deux couvrent les deux")]
        public void TwoRandom_AmongTwo_HitsBoth()
        {
            Cast(TargetRule.AllEnemies, TargetPick.TwoRandom, new[] { "brute", "brute" }, out var t, seed: 3);
            Assert.That(t.OrderBy(x => x).ToArray(), Is.EqualTo(new[] { 2, 3 }));
        }

        [Test, Description("Vérifie que Pick Default suit le chemin historique sans tirage en plus — parce que la sortie seedée et la cible voulue ne doivent pas changer")]
        public void Default_ChangesNothing_AndDrawsNothing()
        {
            var plain = Cast(TargetRule.SingleEnemy, TargetPick.Default, Three, out var tp, preferred: 3);
            var withStatus = Cast(TargetRule.SingleEnemy, TargetPick.Default, Three, out var tc, preferred: 3, pickStatus: StatusType.Burn);
            Assert.That(tp, Is.EqualTo(new[] { 3 }), "cible voulue respectée");
            Assert.That(tc, Is.EqualTo(tp));
            Assert.That(withStatus.RngProbeForTests(), Is.EqualTo(plain.RngProbeForTests()));
            Cast(TargetRule.AllEnemies, TargetPick.Default, Three, out var all);
            Assert.That(all, Is.EqualTo(new[] { 2, 3, 4 }));
        }

        [Test, Description("Vérifie qu'un Pick sur une règle incompatible (Self, AllAllies) est ignoré — parce que Pick ne vaut que pour SingleEnemy, SingleAlly et AllEnemies")]
        public void Pick_IsIgnoredOnIncompatibleRules()
        {
            Cast(TargetRule.Self, TargetPick.Random, Three, out var self);
            Assert.That(self, Is.EqualTo(new[] { 0 }));
            Cast(TargetRule.AllAllies, TargetPick.LowestHp, Three, out var allies);
            Assert.That(allies, Is.EqualTo(new[] { 0, 1 }));
        }

        [Test, Description("Vérifie que MapEffects, WithAbilities, FromAbilities, l'échelle de niveau et les bonus de carte conservent Pick et PickStatus — parce que la compétence reconstruite doit viser pareil")]
        public void MapEffects_KeepsThePick()
        {
            var skill = new SkillDef(new SkillId("s"), SkillSlot.A3, 1, TargetRule.SingleEnemy, new[] { new EffectDef(EffectType.Damage, 10) }, 5, TargetPick.StatusHolder, StatusType.Burn);
            Assert.That(skill.Pick, Is.EqualTo(TargetPick.StatusHolder));
            Assert.That(skill.PickStatus, Is.EqualTo(StatusType.Burn));
            void Same(SkillDef d) { Assert.That(d.Pick, Is.EqualTo(TargetPick.StatusHolder)); Assert.That(d.PickStatus, Is.EqualTo(StatusType.Burn)); }
            Same(skill.MapEffects(e => e));
            Same(skill.WithAbilities(new[] { new Ability(AbilityTrigger.OnCast, new[] { new EffectDef(EffectType.Damage, 1) }) }));
            Same(CardLevels.ScalePercent(skill, 150));
            Same(CardBonuses.Apply(skill, 20));
            var fa = SkillDef.FromAbilities(new SkillId("f"), SkillSlot.A3, 1, TargetRule.SingleEnemy, skill.Abilities, 5, TargetPick.LowestHp, StatusType.Freeze);
            Assert.That(fa.Pick, Is.EqualTo(TargetPick.LowestHp));
            Assert.That(fa.PickStatus, Is.EqualTo(StatusType.Freeze));
            Assert.That(new SkillDef(new SkillId("d"), SkillSlot.A3, 1, TargetRule.SingleEnemy, skill.Effects).Pick, Is.EqualTo(TargetPick.Default));
        }
    }
}
