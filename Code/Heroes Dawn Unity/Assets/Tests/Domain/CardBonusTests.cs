#nullable enable
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Domain
{
    /// <summary>Bonus de position (+15 %) et de famille (+5 %) des cartes équipées — bloc A de la 1.0.0.</summary>
    public sealed class CardBonusTests
    {
        static readonly SkillDef Hit = new SkillDef(new SkillId("hit"), SkillSlot.A3, 0, TargetRule.SingleEnemy, new[] { new EffectDef(EffectType.Damage, 120) }, 0);

        static SkillCardDef Card(Family family, CardNature nature, string id = "t_card")
            => new SkillCardDef(new CardId(id), "cards." + id + ".name", family, 1, new SkillDef(new SkillId(id), SkillSlot.A3, 0, TargetRule.SingleEnemy, Hit.Effects, 0), nature);

        static MemberDef Member(Family family, Position position)
            => new MemberDef(new MemberId("m_" + position), "k", family, position, 1, new StatBlock(1000, 100, 100, 100), new[] { Hit, Hit });

        [Test, Description("Vérifie que le bonus vaut 20 % pour un type boosté de la même famille, 15 % pour un type boosté d'une autre famille, 5 % pour un autre type de la même famille et 0 sinon — parce que position et famille se cumulent")]
        public void Percent_cumule_position_et_famille()
        {
            var attaquant = Member(Family.Matter, Position.Attacker);
            Assert.That(CardBonuses.Percent(Card(Family.Matter, CardNature.Offensive), attaquant), Is.EqualTo(20));
            Assert.That(CardBonuses.Percent(Card(Family.Elemental, CardNature.Offensive), attaquant), Is.EqualTo(15));
            Assert.That(CardBonuses.Percent(Card(Family.Matter, CardNature.Defensive), attaquant), Is.EqualTo(5));
            Assert.That(CardBonuses.Percent(Card(Family.Elemental, CardNature.Defensive), attaquant), Is.EqualTo(0));
        }

        [Test, Description("Vérifie qu'un agent Précision à distance booste les cartes Offensif et Contrôle mais pas Défensif ni Soutien — parce que cette position a deux types boostés")]
        public void Un_Ranged_booste_Offensive_et_Control_mais_pas_Defensive()
        {
            var tireur = Member(Family.Perception, Position.Ranged);
            Assert.That(CardBonuses.Percent(Card(Family.Matter, CardNature.Offensive), tireur), Is.EqualTo(15));
            Assert.That(CardBonuses.Percent(Card(Family.Matter, CardNature.Control), tireur), Is.EqualTo(15));
            Assert.That(CardBonuses.Percent(Card(Family.Matter, CardNature.Defensive), tireur), Is.EqualTo(0));
            Assert.That(CardBonuses.Percent(Card(Family.Matter, CardNature.Support), tireur), Is.EqualTo(0));
        }

        [Test, Description("Vérifie que le bonus ne dépasse jamais 20 % pour aucune carte du catalogue sur aucune position — parce que position et famille sont les deux seuls bonus")]
        public void Le_bonus_ne_depasse_jamais_20_pourcent()
        {
            foreach (var card in PrototypeCollection.Build().Cards)
                foreach (Position p in System.Enum.GetValues(typeof(Position)))
                    foreach (Family f in System.Enum.GetValues(typeof(Family)))
                        Assert.That(CardBonuses.Percent(card, Member(f, p)), Is.InRange(0, 20), card.Id.Value + " " + p + " " + f);
        }

        static CombatUnit Ally(string memberId, SkillCardDef card)
        {
            var catalog = PrototypeContent.Build(new BalanceConfig());
            var col = catalog.Collection;
            catalog = catalog.WithCollection(new CollectionDef(col.Items, col.Cards.Concat(new[] { card }).ToList(), col.Boosters));
            var entry = new AllyEntry(new MemberId(memberId), 5, 0, 0, null, new CardId?[] { card.Id });
            return UnitFactory.Ally(0, catalog.Member(entry.Member), entry, catalog.Balance, catalog.Collection);
        }

        [Test, Description("Vérifie qu'un attaquant (Mei-Lin Chen, Élémentaire) avec une carte offensive de la famille Matière à 120 % de dégâts inflige 138 % — parce que le bonus de position de +15 % s'applique aux cartes d'une autre famille")]
        public void Un_attaquant_avec_une_carte_offensive_inflige_15_pourcent_de_plus()
        {
            var u = Ally("mei_lin_chen", Card(Family.Matter, CardNature.Offensive));
            Assert.That(u.Skills[2].BaseEffect!.Value, Is.EqualTo(138));
        }

        [Test, Description("Vérifie que la même carte de la famille de l'agent reçoit 20 % (144) et qu'une carte défensive d'une autre famille ne reçoit rien (120) — parce que famille et position se cumulent séparément")]
        public void Famille_et_position_se_cumulent_sur_les_degats()
        {
            Assert.That(Ally("mei_lin_chen", Card(Family.Elemental, CardNature.Offensive)).Skills[2].BaseEffect!.Value, Is.EqualTo(144));
            Assert.That(Ally("mei_lin_chen", Card(Family.Matter, CardNature.Defensive)).Skills[2].BaseEffect!.Value, Is.EqualTo(120));
        }

        [Test, Description("Vérifie qu'un ennemi ne reçoit jamais le bonus de position ni de famille — parce que le bonus récompense le choix de cartes du joueur")]
        public void Un_ennemi_ne_recoit_pas_le_bonus()
        {
            var catalog = PrototypeContent.Build(new BalanceConfig());
            var def = catalog.Enemy(new EnemyId("thug"));
            var unit = UnitFactory.Enemy(1, def, new EnemyEntry(def.Id, 5), CombatRank.Normal, catalog.Balance);
            Assert.That(unit.Skills.Select(s => s.BaseEffect!.Value), Is.EqualTo(def.Skills.Select(s => s.BaseEffect!.Value)));
        }

        [Test, Description("Vérifie que le bonus augmente aussi la chance d'une altération (plafonnée à 100 %) et le recul — parce que le bonus s'applique à la valeur principale de tous les types de carte")]
        public void Le_bonus_s_applique_aux_alterations_et_au_recul()
        {
            var skill = new SkillDef(new SkillId("c"), SkillSlot.A3, 0, TargetRule.SingleEnemy,
                new[] { new EffectDef(EffectType.PushBack, 20), new EffectDef(EffectType.ApplyStatus, 0, StatusType.Bind, 2, 50), new EffectDef(EffectType.ApplyStatus, 0, StatusType.Mark, 2, 95) }, 0);
            var scaled = CardBonuses.Apply(skill, 20);
            Assert.That(scaled.Effects[0].Value, Is.EqualTo(24));
            Assert.That(scaled.Effects[1].ChancePercent, Is.EqualTo(60));
            Assert.That(scaled.Effects[2].ChancePercent, Is.EqualTo(100));
            Assert.That(CardBonuses.Apply(skill, 0), Is.SameAs(skill));
        }
    }
}
