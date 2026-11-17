#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Domain.Combat;
using N29.HeroesDawn.Presentation.Combat;
using N29.HeroesDawn.Presentation.Mvp;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Traduction d'un aperçu du moteur en données d'affichage : deltas de PV, statuts, K.O., fantômes de frise, issue du combat.</summary>
    public sealed class CombatPreviewMapperTests
    {
        sealed class StubLoc : ILocalization
        {
            public string T(string key) => key;
            public string F(string key, params object[] args) => key + ":" + string.Join(",", args);
            public bool Has(string key) => true;
            public string Or(string key, string fallbackKey) => key;
            public string Error(DomainError e) => "";
            public string MemberName(MemberDef d) => "";
            public string EnemyName(EnemyDef d) => "";
            public string SkillName(SkillDef s) => "";
            public string SynergyName(SynergyDef s) => "";
            public string SynergyEffect(SynergyDef s) => "";
            public string SynergyEffect(SynergyDef s, int linkLevel) => "";
            public string TeamEffect(TeamEffectKind kind) => "";
            public string PairTheme(Position a, Position b) => "";
            public string PairBonusText(PairBonus bonus) => "";
            public string StatusName(StatusType s) => s.ToString();
            public string FamilyName(Family f) => "";
            public string RoleName(Position r) => "";
            public string RarityName(int rank) => "";
            public string RankName(CombatRank r) => "";
            public string DescribeSkill(SkillDef s) => "";
            public string DescribeSkill(SkillDef s, int atk, int maxHp) => "";
        }

        static UnitPreview Unit(int id, int hpBefore = 100, int hpAfter = 100, int hpMin = -1, int hpMax = -1, int pointBefore = 5, int pointAfter = 5,
                                int pointMin = -1, int pointMax = -1, int defeat = 0, params StatusChange[] added)
            => new UnitPreview
            {
                Id = id, HpBefore = hpBefore, MaxHp = 100, HpAfter = hpAfter, HpMin = hpMin < 0 ? hpAfter : hpMin, HpMax = hpMax < 0 ? hpAfter : hpMax,
                PointBefore = pointBefore, PointAfter = pointAfter, PointMin = pointMin < 0 ? pointAfter : pointMin, PointMax = pointMax < 0 ? pointAfter : pointMax,
                DefeatPercent = defeat, PlannedBefore = -1, Added = added,
            };

        static CombatPreviewResult Result(int victory = 0, int nextWave = 0, params UnitPreview[] units)
            => new CombatPreviewResult(units.ToList(), 16, victory, 0, nextWave);

        static readonly Func<int, bool> IsAlly = id => id == 0;

        [Test, Description("Vérifie qu'une unité sans changement n'apparaît pas dans l'aperçu — parce que seul ce que la carte modifie doit s'afficher")]
        public void Unchanged_UnitsAreOmitted()
        {
            var data = CombatPreviewMapper.Build(Result(0, 0, Unit(1), Unit(2, hpAfter: 70)), new StubLoc(), IsAlly);
            Assert.That(data.Units.Select(u => u.Id), Is.EqualTo(new[] { 2 }));
        }

        [Test, Description("Vérifie le delta de PV : fractions avant/après, texte signé — parce que c'est la barre fantôme et le chiffre affichés sur la fiche")]
        public void HpDelta_IsSignedAndFractional()
        {
            var u = CombatPreviewMapper.Build(Result(0, 0, Unit(1, hpBefore: 80, hpAfter: 50)), new StubLoc(), IsAlly).Units.Single();
            Assert.That(u.HpBefore01, Is.EqualTo(0.8f).Within(0.001f));
            Assert.That(u.HpAfter01, Is.EqualTo(0.5f).Within(0.001f));
            Assert.That(u.HpDeltaText, Is.EqualTo("−30"));
            Assert.That(u.Variable, Is.False);
            Assert.That(u.VariableLabel, Is.Empty);
        }

        [Test, Description("Vérifie que des PV variables donnent la moyenne précédée de ≈ et le badge « variable » — parce que l'utilisateur a choisi une valeur lisible plutôt qu'une plage")]
        public void VariableHp_ShowsAverageAndBadge()
        {
            var u = CombatPreviewMapper.Build(Result(0, 0, Unit(1, hpBefore: 100, hpAfter: 72, hpMin: 65, hpMax: 80)), new StubLoc(), IsAlly).Units.Single();
            Assert.That(u.HpDeltaText, Is.EqualTo("≈ −28"));
            Assert.That(u.Variable, Is.True);
            Assert.That(u.VariableLabel, Is.EqualTo("preview.variable"));
        }

        [Test, Description("Vérifie qu'une unité sûrement vaincue est marquée K.O. sûr et une probable K.O. possible — parce que la mort doit se voir sans ambiguïté")]
        public void Defeat_SureOrPossible()
        {
            var data = CombatPreviewMapper.Build(Result(0, 0, Unit(1, hpAfter: 0, defeat: 100), Unit(2, hpAfter: 5, defeat: 40)), new StubLoc(), IsAlly);
            Assert.That(data.Units.Single(u => u.Id == 1).Defeat, Is.EqualTo(PreviewDefeat.Sure));
            Assert.That(data.Units.Single(u => u.Id == 2).Defeat, Is.EqualTo(PreviewDefeat.Possible));
        }

        [Test, Description("Vérifie que les statuts ajoutés à 100 % sont sûrs et les autres « possibles » — parce que les statuts aléatoires s'affichent en pointillé")]
        public void StatusChips_CarryCertainty()
        {
            var u = CombatPreviewMapper.Build(Result(0, 0, Unit(1, hpAfter: 90, added: new[] { new StatusChange(StatusType.Freeze, 100), new StatusChange(StatusType.Burn, 40) })), new StubLoc(), IsAlly).Units.Single();
            Assert.That(u.Added.Single(c => c.Type == StatusType.Freeze).Possible, Is.False);
            Assert.That(u.Added.Single(c => c.Type == StatusType.Burn).Possible, Is.True);
            Assert.That(u.Added.Single(c => c.Type == StatusType.Burn).Label, Is.EqualTo("Burn"));
        }

        [Test, Description("Vérifie qu'un recul sur la frise donne un fantôme ◀ N, une avance ▶ N, rien si la position ne change pas — parce que ce sont les fantômes de la frise")]
        public void TimelineGhosts_ShowBackAndForward()
        {
            var data = CombatPreviewMapper.Build(Result(0, 0, Unit(1, pointBefore: 6, pointAfter: 3), Unit(2, pointBefore: 2, pointAfter: 5), Unit(0, pointBefore: 10, pointAfter: 0)), new StubLoc(), IsAlly);
            var back = data.Ghosts.Single(g => g.UnitId == 1);
            Assert.That((back.FromPoint, back.ToPoint, back.Text), Is.EqualTo((6, 3, "◀ 3")));
            Assert.That(back.Ally, Is.False);
            var fwd = data.Ghosts.Single(g => g.UnitId == 2);
            Assert.That((fwd.ToPoint, fwd.Text), Is.EqualTo((5, "▶ 3")));
            Assert.That(data.Ghosts.Single(g => g.UnitId == 0).Ally, Is.True, "le lanceur est replacé");
            Assert.That(CombatPreviewMapper.Build(Result(0, 0, Unit(1, hpAfter: 80, pointBefore: 4, pointAfter: 4)), new StubLoc(), IsAlly).Ghosts, Is.Empty);
        }

        [Test, Description("Vérifie qu'une position variable est signalée sur le fantôme — parce que le recul aléatoire doit paraître variable")]
        public void VariableGhost_IsFlagged()
        {
            var g = CombatPreviewMapper.Build(Result(0, 0, Unit(1, pointBefore: 8, pointAfter: 6, pointMin: 5, pointMax: 8)), new StubLoc(), IsAlly).Ghosts.Single();
            Assert.That(g.Variable, Is.True);
            Assert.That(g.Text, Does.StartWith("≈"));
        }

        [Test, Description("Vérifie l'issue du combat : victoire sûre, possible, vague suivante — parce que l'aperçu annonce la fin du combat")]
        public void Outcome_IsAnnounced()
        {
            Assert.That(CombatPreviewMapper.Build(Result(100), new StubLoc(), IsAlly).Outcome, Is.EqualTo("preview.victory"));
            Assert.That(CombatPreviewMapper.Build(Result(30), new StubLoc(), IsAlly).Outcome, Is.EqualTo("preview.victory_maybe"));
            Assert.That(CombatPreviewMapper.Build(Result(0, 100), new StubLoc(), IsAlly).Outcome, Is.EqualTo("preview.next_wave"));
            Assert.That(CombatPreviewMapper.Build(Result(), new StubLoc(), IsAlly).Outcome, Is.Empty);
        }

        [Test, Description("Vérifie qu'une action ennemie annoncée puis annulée est signalée — parce que l'interruption d'une préparation compte autant qu'un recul")]
        public void CancelledPlan_IsNoted()
        {
            var unit = Unit(1, hpAfter: 90);
            unit.PlannedBefore = 1; unit.PlanCancelledPercent = 100;
            var u = CombatPreviewMapper.Build(Result(0, 0, unit), new StubLoc(), IsAlly).Units.Single();
            Assert.That(u.PlanText, Is.EqualTo("preview.plan_cancelled"));
            unit.PlanCancelledPercent = 50;
            Assert.That(CombatPreviewMapper.Build(Result(0, 0, unit), new StubLoc(), IsAlly).Units.Single().PlanText, Is.EqualTo("preview.plan_cancelled_maybe"));
        }
    }
}
