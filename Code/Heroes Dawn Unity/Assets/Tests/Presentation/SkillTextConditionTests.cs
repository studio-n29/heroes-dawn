#nullable enable
using System;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using N29.HeroesDawn.Presentation.Hub;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Textes générés des conditions, modificateurs et ciblage fin (plan P1, tâche 6).</summary>
    public sealed class SkillTextConditionTests
    {
        static EffectDef Dmg(int v = 100, Condition? when = null, params ValueMod[] mods) => new EffectDef(EffectType.Damage, v, when: when, mods: mods);
        static string[] Lines(params EffectDef[] fx) => SkillText.Effects(fx).ToArray();
        static SkillDef Skill(TargetPick pick, StatusType pickStatus = StatusType.None)
            => new SkillDef(new SkillId("x"), SkillSlot.A1, 0, TargetRule.SingleEnemy, new[] { Dmg() }, pick: pick, pickStatus: pickStatus);

        [Test, Description("Vérifie qu'un effet sans condition ni modificateur garde exactement le texte d'avant (cartes Braise et Mur de verre) — parce que le texte existant ne doit pas bouger")]
        public void PlainEffects_KeepExactTextOfExistingCards()
        {
            var col = PrototypeCollection.Build();
            var ember = col.Card(new CardId("card_ember"));
            var wall = col.Card(new CardId("card_glass_wall"));
            int threshold = new BalanceConfig().ExecuteThresholdPercent;
            Assert.That(SkillText.Describe(ember.Skill), Is.EqualTo("1 ennemi · Dégâts 140 %"));
            Assert.That(ember.Tiers.Select(CollectionText.TierEffects),
                Is.EqualTo(new[] { "Brûlure 2 t", "Rebond 50 %", "Exécution +50 % sous " + threshold + " % PV" }));
            Assert.That(SkillText.Describe(wall.Skill), Is.EqualTo("Toute l'équipe · Bouclier 15 % PV"));
            Assert.That(wall.Tiers.Select(CollectionText.TierEffects), Is.EqualTo(new[] { "Purge (1)", "Ancrage 1 t", "Régén. 2 t" }));
        }

        [Test, Description("Vérifie le texte exact d'un modificateur fixe sous condition de statut — parce que le joueur doit lire « +40 % si la cible subit Brûlure »")]
        public void FlatMod_TargetHasStatus_ExactText()
        {
            var lines = Lines(Dmg(100, null, new ValueMod(40, new Condition(ConditionKind.TargetHasStatus, StatusType.Burn))));
            Assert.That(lines, Is.EqualTo(new[] { "Dégâts 100 %", "+40 % si la cible subit Brûlure" }));
        }

        [Test, Description("Vérifie le texte exact d'une condition de PV sur un effet conditionnel — parce que l'effet doit être préfixé « Si … : »")]
        public void ConditionalEffect_HpBelow_ExactText()
        {
            var lines = Lines(Dmg(80, new Condition(ConditionKind.TargetHpBelow, StatusType.None, 50)));
            Assert.That(lines, Is.EqualTo(new[] { "Si la cible est sous 50 % PV : Dégâts 80 %" }));
        }

        [Test, Description("Vérifie le texte exact d'un modificateur à échelle plafonné — parce que le joueur doit voir le bonus par unité et le maximum")]
        public void ScaledMod_ExactText()
        {
            var lines = Lines(Dmg(100, null, new ValueMod(15, null, ScaleKind.TargetStatusCount, 3)));
            Assert.That(lines, Is.EqualTo(new[] { "Dégâts 100 %", "+15 % par statut sur la cible (max 3)" }));
        }

        [Test, Description("Vérifie qu'une condition produit une phrase lisible, sans clé manquante, pour chaque ConditionKind — parce qu'un texte « [cond.X] » serait affiché au joueur")]
        public void EveryConditionKind_ProducesReadableSentence()
        {
            foreach (ConditionKind k in Enum.GetValues(typeof(ConditionKind)))
            {
                var lines = Lines(Dmg(100, new Condition(k, StatusType.Burn, 2)));
                Assert.That(lines, Has.Length.EqualTo(1), k.ToString());
                Assert.That(lines[0], Does.StartWith("Si ").And.Not.Contain("["), k.ToString());
            }
        }

        [Test, Description("Vérifie qu'un modificateur à échelle produit une phrase lisible, sans clé manquante, pour chaque ScaleKind — parce qu'un texte « [scale.X] » serait affiché au joueur")]
        public void EveryScaleKind_ProducesReadableSentence()
        {
            foreach (ScaleKind k in Enum.GetValues(typeof(ScaleKind)))
            {
                var lines = Lines(Dmg(100, null, new ValueMod(10, null, k, 2)));
                Assert.That(lines, Has.Length.EqualTo(2), k.ToString());
                Assert.That(lines[1], Does.StartWith("+10 %").And.Contain("(max 2)").And.Not.Contain("["), k.ToString());
            }
        }

        [Test, Description("Vérifie que Describe mentionne le ciblage fin, avec le statut cherché pour StatusHolder, et rien pour le ciblage par défaut — parce que le joueur doit savoir qui sera visé")]
        public void Describe_MentionsFinePick()
        {
            Assert.That(SkillText.Describe(Skill(TargetPick.LowestHp)), Is.EqualTo("1 ennemi · Cible : le plus faible en PV · Dégâts 100 %"));
            Assert.That(SkillText.Describe(Skill(TargetPick.StatusHolder, StatusType.Burn)), Does.Contain("Cible : porteur de Brûlure"));
            Assert.That(SkillText.Describe(Skill(TargetPick.Default)), Is.EqualTo("1 ennemi · Dégâts 100 %"));
        }

        [Test, Description("Vérifie que la ligne d'un palier de carte montre l'effet conditionnel — parce que le grimoire passe par TierEffects")]
        public void TierLine_ShowsConditionalEffect()
        {
            var tier = new CardTier(3, Dmg(60, new Condition(ConditionKind.TargetHasStatus, StatusType.Burn)));
            Assert.That(CollectionText.TierEffects(tier), Is.EqualTo("Si la cible subit Brûlure : Dégâts 60 %"));
        }

        [Test, Description("Vérifie qu'une clé de texte existe pour chaque ConditionKind, ScaleKind et TargetPick (hors Default) — parce qu'une valeur ajoutée plus tard sans texte afficherait une clé brute")]
        public void AllKeysExist_ForEveryKind()
        {
            foreach (ConditionKind k in Enum.GetValues(typeof(ConditionKind))) Assert.That(Loc.Has("cond." + k), "cond." + k);
            foreach (ScaleKind k in Enum.GetValues(typeof(ScaleKind))) Assert.That(Loc.Has("scale." + k), "scale." + k);
            foreach (TargetPick k in Enum.GetValues(typeof(TargetPick)).Cast<TargetPick>().Where(p => p != TargetPick.Default)) Assert.That(Loc.Has("pick." + k), "pick." + k);
            Assert.That(Loc.Has("fx.mod_flat") && Loc.Has("fx.mod_scaled") && Loc.Has("fx.conditional"));
        }
    }
}
