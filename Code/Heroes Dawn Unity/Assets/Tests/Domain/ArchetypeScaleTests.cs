#nullable enable
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Échelle d'archétype (P3) : bonus par carte équipée de l'archétype dans le camp du lanceur (carte lancée comprise), plafonné, sans tirage au sort.</summary>
    public sealed class ArchetypeScaleTests
    {
        static readonly string[] AllyIds = { "verre", "givre", "ancre" };
        static readonly string[] One = { "brute" };

        /// <summary>Carte élémentaire (ni famille ni position de « verre » : aucun bonus de carte sur les dégâts), coût 0, un seul effet de dégâts.</summary>
        static SkillCardDef Card(string id, Archetype? archetype, EffectDef effect)
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", Family.Elemental, 1,
                                new SkillDef(new SkillId(id), SkillSlot.A3, 0, TargetRule.SingleEnemy, new[] { effect }, 0), CardNature.Offensive,
                                null, archetype.HasValue ? new[] { archetype.Value } : null);

        static EffectDef Damage(int value, params ValueMod[] mods) => new EffectDef(EffectType.Damage, value, mods: mods);
        static ValueMod Scale(int bonus, int max, Archetype a = Archetype.Burn) => new ValueMod(bonus, null, ScaleKind.EquippedArchetypeCards, max, a);
        static SkillCardDef Filler(string id, Archetype? a) => Card(id, a, Damage(10));

        static CombatSimulation Fight(SkillCardDef[][] cardsPerAlly, SkillCardDef[][]? opponents = null, bool auto = false, ulong seed = 11)
        {
            var c = TestContent.Build();
            var col = c.Collection;
            var all = cardsPerAlly.Concat(opponents ?? new SkillCardDef[0][]).SelectMany(x => x).ToList();
            var catalog = c.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(all).ToList(), col.Boosters));
            AllyEntry[] Team(SkillCardDef[][] teamCards) => teamCards.Select((cards, i) => new AllyEntry(new MemberId(AllyIds[i]), 5, 0, 0, null,
                cards.Select(k => (CardId?)k.Id).ToArray(), cards.Select(_ => 1).ToArray())).ToArray();
            var setup = new CombatSetup(seed, CombatRank.Normal, Team(cardsPerAlly), opponents == null ? One.Select(e => new EnemyEntry(new EnemyId(e), 5)).ToArray() : new EnemyEntry[0], auto);
            if (opponents != null) setup.WithOpponentTeam(Team(opponents), SideControl.Ai);
            var sim = new CombatSimulation(catalog, setup);
            foreach (var u in sim.Units) u.Point = 0;
            return sim;
        }

        /// <summary>L'unité lance la première carte de son kit (emplacement 2) sur la cible ; renvoie les dégâts infligés à celle-ci.</summary>
        static int CastDamage(CombatSimulation sim, int actor, int target)
        {
            var u = sim.Unit(actor);
            u.Point = u.ActionPoint;
            u.Energy = u.MaxEnergy;
            sim.Submit(CombatCommand.UseSkill(actor, 2, target));
            var events = sim.Step(1).ToList();
            Assert.That(events.OfType<SkillUsedEvent>().Any(e => e.Actor == actor), Is.True, "la carte est lancée");
            return events.OfType<DamageEvent>().Where(d => d.Target == target).Sum(d => d.Amount);
        }

        /// <summary>
        /// Dégâts de la carte « card_s » (effet donné) lancée par l'allié 0 sur le premier ennemi, l'escouade portant en plus les cartes données (la carte lancée est ajoutée au premier agent).
        /// Pour comparer deux effets, l'escouade doit être identique : le nombre d'unités change les tirages de départ, donc les dégâts.
        /// </summary>
        static int Run(EffectDef castEffect, SkillCardDef[][] others, SkillCardDef[][]? opponents = null, System.Action<CombatSimulation>? before = null)
        {
            var cast = Card("card_s", Archetype.Burn, castEffect);
            var squad = others.Select((x, i) => i == 0 ? new[] { cast }.Concat(x).ToArray() : x).ToArray();
            var sim = Fight(squad, opponents);
            before?.Invoke(sim);
            return CastDamage(sim, 0, squad.Length); // première unité ennemie
        }

        static int Scaled(ValueMod mod, SkillCardDef[][] others, SkillCardDef[][]? opponents = null, System.Action<CombatSimulation>? before = null)
            => Run(Damage(100, mod), others, opponents, before);

        static int Fixed(int value, SkillCardDef[][] others, SkillCardDef[][]? opponents = null, System.Action<CombatSimulation>? before = null)
            => Run(Damage(value), others, opponents, before);

        static readonly SkillCardDef[][] Alone = { new SkillCardDef[0] };

        [Test, Description("Vérifie que l'échelle compte les cartes de l'archétype équipées dans toute l'escouade, carte lancée comprise (3 cartes Brûlure : +30), même quand une unité de l'escouade est vaincue — parce que le bonus vaut Bonus × nombre de cartes de l'archétype et que la liste est figée au départ du combat")]
        public void ArchetypeScale_CountsEquippedCardsOfTheSquad_CasterCardIncluded()
        {
            var others = new[] { new[] { Filler("card_a", Archetype.Burn) }, new[] { Filler("card_b", Archetype.Burn) } };
            Assert.That(Scaled(Scale(10, 6), others), Is.EqualTo(Fixed(130, others)), "3 cartes Brûlure : 100 + 3 × 10");
            Assert.That(Scaled(Scale(10, 6), Alone), Is.EqualTo(Fixed(110, Alone)), "la seule carte lancée compte déjà : 100 + 1 × 10");
            Assert.That(Scaled(Scale(10, 6), others), Is.Not.EqualTo(Fixed(110, others)), "le bonus de la carte lancée seule serait différent");
            System.Action<CombatSimulation> kill = s => s.Unit(1).Hp = 0;
            Assert.That(Scaled(Scale(10, 6), others, null, kill), Is.EqualTo(Fixed(130, others, null, kill)), "une unité vaincue garde ses cartes dans le compte");
        }

        [Test, Description("Vérifie que l'échelle est plafonnée par MaxCount (3 cartes Brûlure, max 2 : +20 seulement) — parce que le bonus vaut Bonus × min(compte, MaxCount)")]
        public void ArchetypeScale_IsCappedByMaxCount()
        {
            var others = new[] { new[] { Filler("card_a", Archetype.Burn) }, new[] { Filler("card_b", Archetype.Burn) } };
            Assert.That(Scaled(Scale(10, 2), others), Is.EqualTo(Fixed(120, others)));
        }

        [Test, Description("Vérifie que seules les cartes de l'archétype demandé comptent (une carte Traque ne compte pas pour Brûlure, une carte hybride Brûlure+Traque compte pour les deux) — parce que le compte est propre à l'archétype nommé par le modificateur")]
        public void ArchetypeScale_IgnoresOtherArchetypes()
        {
            var others = new[] { new[] { Filler("card_a", Archetype.Hunt) }, new[] { Filler("card_b", null) } };
            Assert.That(Scaled(Scale(10, 6), others), Is.EqualTo(Fixed(110, others)), "seule la carte lancée est Brûlure");
            Assert.That(Scaled(Scale(10, 6, Archetype.Hunt), others), Is.EqualTo(Fixed(110, others)), "une seule carte Traque (la lancée est Brûlure)");
            var hybrid = new SkillCardDef(new CardId("card_h"), "cards.card_h.name", Family.Elemental, 1,
                new SkillDef(new SkillId("card_h"), SkillSlot.A3, 0, TargetRule.SingleEnemy, new[] { Damage(10) }, 0), CardNature.Offensive, null, new[] { Archetype.Burn, Archetype.Hunt });
            var squad = new[] { new[] { hybrid }, new SkillCardDef[0] };
            Assert.That(Scaled(Scale(10, 6, Archetype.Hunt), squad), Is.EqualTo(Fixed(110, squad)), "la carte hybride compte pour Traque");
            Assert.That(Scaled(Scale(10, 6, Archetype.Burn), squad), Is.EqualTo(Fixed(120, squad)), "la carte hybride compte aussi pour Brûlure (avec la carte lancée)");
        }

        [Test, Description("Vérifie que seul le camp du lanceur est compté : les cartes Brûlure de l'équipe adverse (duel) ne comptent pas pour l'allié, et inversement — parce que l'échelle porte sur l'escouade du lanceur")]
        public void ArchetypeScale_CountsOnlyTheCastersSide()
        {
            var opponents = new[] { new[] { Filler("card_o1", Archetype.Burn), Filler("card_o2", Archetype.Burn) }, new[] { Filler("card_o3", Archetype.Burn) } };
            Assert.That(Scaled(Scale(10, 6), Alone, opponents), Is.EqualTo(Fixed(110, Alone, opponents)), "les trois cartes adverses ne comptent pas : seule la carte lancée");

            SkillCardDef[][] Mine() => new[] { new[] { Card("card_s", Archetype.Burn, Damage(100, Scale(10, 6))), Filler("card_a", Archetype.Burn) } };
            SkillCardDef[][] Theirs(EffectDef fx) => new[] { new[] { Card("card_e", Archetype.Burn, fx) }, new[] { Filler("card_o3", Archetype.Burn) } };
            var sim = Fight(Mine(), Theirs(Damage(100, Scale(10, 6))));
            Assert.That(sim.Unit(0).EquippedArchetypes, Is.EqualTo(new[] { Archetype.Burn, Archetype.Burn }), "liste de l'allié");
            Assert.That(sim.Unit(1).Side, Is.EqualTo(Side.Enemy));
            Assert.That(sim.Unit(1).EquippedArchetypes, Is.EqualTo(new[] { Archetype.Burn }), "l'équipe adverse reçoit la même liste");
            Assert.That(sim.Unit(2).EquippedArchetypes, Is.EqualTo(new[] { Archetype.Burn }));
            // L'unité adverse 1 compte les cartes de son camp (la sienne et celle de l'unité 2), pas les deux de l'allié : 100 + 2 × 10.
            Assert.That(CastDamage(sim, 1, 0), Is.EqualTo(CastDamage(Fight(Mine(), Theirs(Damage(120))), 1, 0)));
        }

        [Test, Description("Vérifie que les ennemis de campagne n'ont aucune carte équipée (compte nul) — parce que les ennemis ne portent pas de grimoire")]
        public void CampaignEnemies_HaveNoEquippedArchetypes()
        {
            var sim = Fight(new[] { new[] { Filler("card_a", Archetype.Burn) } });
            Assert.That(sim.Unit(0).EquippedArchetypes, Is.EqualTo(new[] { Archetype.Burn }));
            Assert.That(sim.Unit(1).Side, Is.EqualTo(Side.Enemy));
            Assert.That(sim.Unit(1).EquippedArchetypes, Is.Empty);
        }

        static string Run(CombatSimulation sim)
        {
            for (int i = 0; i < 600 && !sim.IsOver; i++) sim.Step(1);
            return CombatDigest.Of(sim);
        }

        [Test, Description("Vérifie qu'un combat sans échelle d'archétype est identique à l'octet (événements et générateur), que les cartes portent un archétype ou non — parce que la liste des cartes équipées ne doit rien changer sans échelle")]
        public void NoArchetypeScale_ChangesNothing()
        {
            SkillCardDef[][] Squad(Archetype? a) => new[] { new[] { Card("card_p", a, Damage(100)), Card("card_q", a, Damage(60)) }, new[] { Card("card_r", a, Damage(80)) } };
            var plain = Fight(Squad(null), auto: true);
            var tagged = Fight(Squad(Archetype.Burn), auto: true);
            string digest = Run(plain);
            Assert.That(plain.Log.OfType<SkillUsedEvent>().Any(e => e.Skill.Value == "card_p" || e.Skill.Value == "card_q" || e.Skill.Value == "card_r"), Is.True, "une carte est jouée par l'IA");
            Assert.That(Run(tagged), Is.EqualTo(digest));
        }

        [Test, Description("Vérifie qu'une échelle d'archétype donne le même combat (événements et générateur) qu'un bonus fixe de même valeur — parce que le compte ne tire aucun nombre au hasard")]
        public void ArchetypeScale_DrawsNoRandomNumber()
        {
            SkillCardDef[][] Squad(EffectDef fx) => new[] { new[] { Card("card_s", Archetype.Burn, fx), Card("card_a", Archetype.Burn, Damage(60)) }, new[] { Card("card_b", Archetype.Burn, Damage(80)) } };
            var scaled = Fight(Squad(Damage(100, Scale(10, 6))), auto: true);
            var fixedBonus = Fight(Squad(Damage(130)), auto: true);
            string digest = Run(fixedBonus);
            Assert.That(Run(scaled), Is.EqualTo(digest));
            Assert.That(fixedBonus.Log.OfType<SkillUsedEvent>().Any(e => e.Skill.Value == "card_s"), Is.True, "la carte à échelle est jouée par l'IA");
        }
    }
}
