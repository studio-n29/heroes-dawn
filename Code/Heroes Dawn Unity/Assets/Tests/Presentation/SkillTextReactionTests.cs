#nullable enable
using System;
using System.Linq;
using N29.HeroesDawn.Content.Prototype;
using N29.HeroesDawn.Core;
using N29.HeroesDawn.Definitions;
using N29.HeroesDawn.Presentation.Common;
using NUnit.Framework;

namespace N29.HeroesDawn.Tests.Presentation
{
    /// <summary>Textes générés des réactions armées, de Consomme, de Propage et de l'échelle d'archétype (plan P2-P4, tâche 4).</summary>
    public sealed class SkillTextReactionTests
    {
        static string[] Lines(params EffectDef[] fx) => SkillText.Effects(fx).ToArray();

        static EffectDef Arm(ReactionTrigger trigger, EffectDef effect, StatusType status = StatusType.None, int turns = 3,
                             TargetRule target = TargetRule.Self, TargetPick pick = TargetPick.Default, StatusType pickStatus = StatusType.None, bool onTrigger = false)
            => new EffectDef(EffectType.ArmReaction, 0, reaction: new ReactionDef(trigger, status, turns, effect, target, pick, pickStatus, onTrigger));

        static EffectDef Dmg(int v) => new EffectDef(EffectType.Damage, v);

        // ------------------------------------------------------------------ Réactions : événements

        [Test, Description("Vérifie la phrase exacte d'une réaction sur « un ennemi reçoit un statut » avec cible nommée (un ennemi au hasard) — parce que le joueur doit lire l'événement, le statut, la durée, l'effet et la cible")]
        public void Reaction_EnemyReceivesStatus_NamedRandomEnemy()
            => Assert.That(Lines(Arm(ReactionTrigger.EnemyReceivesStatus, Dmg(70), StatusType.Shock, 3, TargetRule.SingleEnemy, TargetPick.Random)),
                           Is.EqualTo(new[] { "Réaction 3 t : quand un ennemi reçoit Choc → Dégâts 70 % sur un ennemi au hasard" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « un allié reçoit un statut » sans cible nommée (l'unité déclencheuse, cible par défaut des deux événements de statut) — parce que la cible implicite ne s'écrit pas")]
        public void Reaction_AllyReceivesStatus_UnnamedTarget()
            => Assert.That(Lines(Arm(ReactionTrigger.AllyReceivesStatus, new EffectDef(EffectType.Heal, 10), StatusType.Burn, 2, onTrigger: true)),
                           Is.EqualTo(new[] { "Réaction 2 t : quand un allié reçoit Brûlure → Soin 10 % PV" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « un ennemi est vaincu » sans cible nommée (le porteur, cible par défaut hors événements de statut), durée d'un seul tour — parce que « 1 t » doit rester correct")]
        public void Reaction_EnemyDefeated_UnnamedTarget_OneTurn()
            => Assert.That(Lines(Arm(ReactionTrigger.EnemyDefeated, new EffectDef(EffectType.Shield, 20), StatusType.None, 1)),
                           Is.EqualTo(new[] { "Réaction 1 t : quand un ennemi est vaincu → Bouclier 20 % PV" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « un allié est vaincu » visant toute l'équipe — parce que la zone alliée s'écrit « toute l'équipe »")]
        public void Reaction_AllyDefeated_WholeTeam()
            => Assert.That(Lines(Arm(ReactionTrigger.AllyDefeated, new EffectDef(EffectType.Advance, 15), StatusType.None, 4, TargetRule.AllAllies)),
                           Is.EqualTo(new[] { "Réaction 4 t : quand un allié est vaincu → Propulse la cible de 15 % vers l'action sur toute l'équipe" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « le porteur est touché » visant le déclencheur (l'attaquant), donc nommée puisque le défaut serait le porteur — parce que « sur le déclencheur » doit apparaître")]
        public void Reaction_CarrierHit_OnTheTriggerUnit()
            => Assert.That(Lines(Arm(ReactionTrigger.CarrierHit, new EffectDef(EffectType.ApplyStatus, 0, StatusType.Burn, 2), StatusType.None, 5, onTrigger: true)),
                           Is.EqualTo(new[] { "Réaction 5 t : quand le porteur est touché → Brûlure 2 t sur le déclencheur" }));

        [Test, Description("Vérifie la phrase exacte d'une réaction « le porteur inflige un critique » visant tous les ennemis — parce que la zone ennemie s'écrit « tous les ennemis »")]
        public void Reaction_CarrierCrit_AllEnemies()
            => Assert.That(Lines(Arm(ReactionTrigger.CarrierCrit, Dmg(50), StatusType.None, 3, TargetRule.AllEnemies)),
                           Is.EqualTo(new[] { "Réaction 3 t : quand le porteur inflige un critique → Dégâts 50 % sur tous les ennemis" }));

        // ------------------------------------------------------------------ Réactions : cibles nommées

        [Test, Description("Vérifie la cible nommée « soi » sur un événement de statut (où le défaut est le déclencheur) — parce que la forme « Soi » de la fiche doit se lire")]
        public void Reaction_NamedSelf_OnAStatusEvent()
            => Assert.That(Lines(Arm(ReactionTrigger.EnemyReceivesStatus, Dmg(30), StatusType.Freeze, 2, TargetRule.Self)),
                           Is.EqualTo(new[] { "Réaction 2 t : quand un ennemi reçoit Gel → Dégâts 30 % sur soi" }));

        [Test, Description("Vérifie la cible nommée « le porteur » (Self) hors événements de statut : elle est implicite, donc non écrite — parce que le défaut ne doit pas alourdir le texte")]
        public void Reaction_SelfOnAnOtherEvent_IsImplicit()
            => Assert.That(Lines(Arm(ReactionTrigger.CarrierHit, new EffectDef(EffectType.Heal, 5), StatusType.None, 2, TargetRule.Self)),
                           Is.EqualTo(new[] { "Réaction 2 t : quand le porteur est touché → Soin 5 % PV" }));

        [TestCase(TargetRule.SingleEnemy, TargetPick.Default, "sur un ennemi"), TestCase(TargetRule.SingleEnemy, TargetPick.Random, "sur un ennemi au hasard"),
         TestCase(TargetRule.SingleEnemy, TargetPick.TwoRandom, "sur deux ennemis au hasard"), TestCase(TargetRule.SingleEnemy, TargetPick.LowestHp, "sur l'ennemi le plus faible"),
         TestCase(TargetRule.SingleEnemy, TargetPick.HighestHp, "sur l'ennemi le plus en forme"), TestCase(TargetRule.SingleEnemy, TargetPick.NextToAct, "sur le prochain ennemi à agir"),
         TestCase(TargetRule.SingleEnemy, TargetPick.StatusHolder, "sur un ennemi porteur de Brûlure"),
         TestCase(TargetRule.SingleAlly, TargetPick.Default, "sur un allié"), TestCase(TargetRule.SingleAlly, TargetPick.Random, "sur un allié au hasard"),
         TestCase(TargetRule.SingleAlly, TargetPick.LowestHp, "sur l'allié le plus faible"), TestCase(TargetRule.SingleAlly, TargetPick.NextToAct, "sur le prochain allié à agir"),
         TestCase(TargetRule.AllEnemies, TargetPick.Random, "sur tous les ennemis"), TestCase(TargetRule.AllAllies, TargetPick.LowestHp, "sur toute l'équipe")]
        [Description("Vérifie la cible nommée de chaque forme (règle et choix) : un ou deux au hasard, le plus faible, le plus en forme, le prochain à agir, porteur d'un statut, zones — parce que la fiche nomme la cible par règle et choix")]
        public void Reaction_NamedTargetForms(TargetRule rule, TargetPick pick, string expected)
            => Assert.That(Lines(Arm(ReactionTrigger.CarrierCrit, Dmg(40), StatusType.None, 3, rule, pick, StatusType.Burn)),
                           Is.EqualTo(new[] { "Réaction 3 t : quand le porteur inflige un critique → Dégâts 40 % " + expected }));

        [Test, Description("Vérifie qu'une compétence qui arme une réaction se décrit en entier (cible de la carte, dégâts, réaction) — parce que la fiche d'une carte passe par Describe")]
        public void Describe_ASkillThatArmsAReaction()
        {
            var skill = new SkillDef(new SkillId("x"), SkillSlot.A3, 0, TargetRule.SingleEnemy,
                new[] { Dmg(100), Arm(ReactionTrigger.EnemyDefeated, new EffectDef(EffectType.Heal, 10), StatusType.None, 2) });
            Assert.That(SkillText.Describe(skill), Is.EqualTo("1 ennemi · Dégâts 100 % · Réaction 2 t : quand un ennemi est vaincu → Soin 10 % PV"));
        }

        [Test, Description("Vérifie qu'en mode propriétaire (montants bruts) l'effet d'une réaction reste en pourcentage — parce que la valeur d'une réaction n'est jamais convertie en montant brut")]
        public void Reaction_OwnerMode_StaysInPercent()
        {
            var owner = new SkillOwnerStats(atk: 196, maxHp: 200);
            var fx = Arm(ReactionTrigger.EnemyReceivesStatus, Dmg(70), StatusType.Shock, 3, TargetRule.SingleEnemy, TargetPick.Random);
            Assert.That(SkillText.Effects(new[] { fx }, owner), Is.EqualTo(Lines(fx)));
        }

        // ------------------------------------------------------------------ Consomme et Propage

        [Test, Description("Vérifie la phrase exacte de Consomme (dégâts par tour restant, plafond de tours) — parce que le joueur doit voir le statut consommé, la valeur par tour et le maximum")]
        public void ConsumeDamage_ExactText()
            => Assert.That(Lines(new EffectDef(EffectType.ConsumeDamage, 40, StatusType.Burn, 3)), Is.EqualTo(new[] { "Consomme Brûlure : Dégâts 40 % par tour restant (max 3)" }));

        [Test, Description("Vérifie la phrase exacte de Consomme en soin (statut Régén., max 5) — parce que le soin s'écrit en % de PV comme les autres soins")]
        public void ConsumeHeal_ExactText()
            => Assert.That(Lines(new EffectDef(EffectType.ConsumeHeal, 30, StatusType.Regen, 5)), Is.EqualTo(new[] { "Consomme Régén. : Soin 30 % PV par tour restant (max 5)" }));

        [Test, Description("Vérifie la phrase de Consomme Marque (max 1) — parce que la forme à un seul tour garde la même tournure")]
        public void ConsumeDamage_Mark_MaxOne()
            => Assert.That(Lines(new EffectDef(EffectType.ConsumeDamage, 150, StatusType.Mark, 1)), Is.EqualTo(new[] { "Consomme Marque : Dégâts 150 % par tour restant (max 1)" }));

        [Test, Description("Vérifie la phrase exacte de Propage — parce que l'effet ne porte que le statut copié")]
        public void Spread_ExactText()
            => Assert.That(Lines(new EffectDef(EffectType.Spread, 0, StatusType.Burn)), Is.EqualTo(new[] { "Propage Brûlure" }));

        [Test, Description("Vérifie qu'en mode propriétaire les textes de Consomme et de Propage restent en pourcentage — parce que ces valeurs sont par tour restant et ne se convertissent pas en montant brut")]
        public void ConsumeAndSpread_OwnerMode_StayInPercent()
        {
            var owner = new SkillOwnerStats(atk: 196, maxHp: 200);
            var fx = new[] { new EffectDef(EffectType.ConsumeDamage, 40, StatusType.Burn, 3), new EffectDef(EffectType.ConsumeHeal, 30, StatusType.Regen, 5), new EffectDef(EffectType.Spread, 0, StatusType.Burn) };
            Assert.That(SkillText.Effects(fx, owner), Is.EqualTo(new[]
            {
                "Consomme Brûlure : Dégâts 40 % par tour restant (max 3)", "Consomme Régén. : Soin 30 % PV par tour restant (max 5)", "Propage Brûlure",
            }));
        }

        // ------------------------------------------------------------------ Échelle d'archétype

        [TestCase(Archetype.Burn, "Brûlure"), TestCase(Archetype.Restrain, "Entrave"), TestCase(Archetype.Hunt, "Traque"),
         TestCase(Archetype.Tempo, "Tempo"), TestCase(Archetype.Bulwark, "Rempart"), TestCase(Archetype.Renewal, "Renouveau")]
        [Description("Vérifie la phrase exacte de l'échelle d'archétype pour chaque archétype (nom français) — parce que le joueur doit lire « par carte Brûlure équipée dans l'escouade (max 4) »")]
        public void ArchetypeScale_ExactSentence_PerArchetype(Archetype a, string name)
        {
            var fx = new EffectDef(EffectType.Damage, 100, mods: new[] { new ValueMod(10, null, ScaleKind.EquippedArchetypeCards, 4, a) });
            Assert.That(Lines(fx), Is.EqualTo(new[] { "Dégâts 100 %", "+10 % par carte " + name + " équipée dans l'escouade (max 4)" }));
            Assert.That(Enum.GetValues(typeof(Archetype)).Length, Is.EqualTo(6), "un Archetype a été ajouté : compléter la table");
        }

        [Test, Description("Vérifie que l'échelle d'archétype se convertit en montant brut comme les autres échelles en mode propriétaire — parce que « +19 de dégâts par carte » doit suivre « Dégâts 196 »")]
        public void ArchetypeScale_OwnerMode_ConvertsToRaw()
        {
            var fx = new EffectDef(EffectType.Damage, 100, mods: new[] { new ValueMod(10, null, ScaleKind.EquippedArchetypeCards, 3, Archetype.Bulwark) });
            Assert.That(SkillText.Effects(new[] { fx }, new SkillOwnerStats(196, 200)), Is.EqualTo(new[] { "Dégâts 196", "+19 de dégâts par carte Rempart équipée dans l'escouade (max 3)" }));
        }

        // ------------------------------------------------------------------ Textes existants et clés

        [Test, Description("Vérifie qu'un effet sans réaction, Consomme, Propage ni échelle d'archétype garde exactement son ancien texte (cartes Braise et Mur de verre) — parce que le texte existant ne doit pas bouger")]
        public void PlainEffects_KeepExactTextOfExistingCards()
        {
            var col = PrototypeCollection.Build();
            var ember = col.Card(new CardId("card_ember"));
            var wall = col.Card(new CardId("card_glass_wall"));
            Assert.That(SkillText.Describe(ember.Skill), Is.EqualTo("1 ennemi · Dégâts 140 %"));
            Assert.That(SkillText.Describe(wall.Skill), Is.EqualTo("Toute l'équipe · Bouclier 15 % PV"));
            Assert.That(Lines(new EffectDef(EffectType.Damage, 100, mods: new[] { new ValueMod(15, null, ScaleKind.TargetStatusCount, 3) })), Is.EqualTo(new[] { "Dégâts 100 %", "+15 % par statut sur la cible (max 3)" }));
        }

        [Test, Description("Vérifie qu'une clé de texte existe pour chaque ReactionTrigger, TargetPick, TargetRule, Archetype et ScaleKind, et pour les gabarits de réaction, Consomme et Propage — parce qu'une valeur ajoutée plus tard sans texte afficherait une clé brute")]
        public void AllKeysExist_ForEveryNewKind()
        {
            foreach (ReactionTrigger k in Enum.GetValues(typeof(ReactionTrigger))) Assert.That(Loc.Has("reaction.trigger." + k), "reaction.trigger." + k);
            foreach (TargetRule k in Enum.GetValues(typeof(TargetRule))) Assert.That(Loc.Has("reaction.target." + k), "reaction.target." + k);
            foreach (TargetPick k in Enum.GetValues(typeof(TargetPick))) Assert.That(Loc.Has("reaction.pick." + k), "reaction.pick." + k);
            foreach (Archetype k in Enum.GetValues(typeof(Archetype))) Assert.That(Loc.Has("archetype." + k), "archetype." + k);
            foreach (ScaleKind k in Enum.GetValues(typeof(ScaleKind))) Assert.That(Loc.Has("scale." + k), "scale." + k);
            foreach (var key in new[] { "reaction.line", "reaction.line_target", "reaction.target.trigger", "reaction.the.SingleEnemy", "reaction.the.SingleAlly", "fx.consume_damage", "fx.consume_heal", "fx.spread" })
                Assert.That(Loc.Has(key), key);
            Assert.That(Enum.GetValues(typeof(ReactionTrigger)).Length, Is.EqualTo(6), "un ReactionTrigger a été ajouté : compléter la table des phrases");
        }
    }
}
